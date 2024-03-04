using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.Configuration;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.Hubs;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;


public record LightningLoginRequest(string Sig, string Key, string K1, string Action)
    : IRequest<LNUrlStatusResponse>;

public record LoginUserResponse(string AccessToken, string RefreshToken);

public class LnLoginHandler(
    AppDbContext dbContext,
    IHubContext<LightningAuthHub, ILightningAuthClient> hub,
    UserManager<AppUser> userManager,
    AuthSettings authSettings)
    : IRequestHandler<LightningLoginRequest, LNUrlStatusResponse>
{
    public Task<LNUrlStatusResponse> Handle(LightningLoginRequest request, CancellationToken cancellationToken)
    {
        var action = Enum.Parse<LnAuthRequest.LnAuthRequestAction>(request.Action, true);

        return action switch
        {
            LnAuthRequest.LnAuthRequestAction.Login => ValidateLogin(request, cancellationToken),
            _ => Task.FromResult(LNUrlStatusResponse.UnknownActionResponse())
        };
    }

    private async Task<LNUrlStatusResponse> ValidateLogin(LightningLoginRequest request,
        CancellationToken cancellationToken)
    {
        var isK1Valid =
            await dbContext.LinkingKeys
                .AnyAsync(x =>
                    x.Type == LinkingKeyTypes.Login && x.K1 == request.K1, cancellationToken);

        if (!isK1Valid)
        {
            return LNUrlStatusResponse.InvalidK1();
        } 
        
        var user =
            await dbContext.Users
                .SingleOrDefaultAsync(x => x.PubKey == request.Key, cancellationToken);
       
        if (user is null)
        {
            await SendMessageToHub(LNUrlStatusResponse.UserNotFound(), user, request.K1);
            return LNUrlStatusResponse.UserNotFound();
        }
       
        var pubKey = new PubKey(request.Key);
        var signature = new ECDSASignature(Encoders.Hex.DecodeData(request.Sig));
        var isOkLogin = LnAuthRequest.VerifyChallenge(signature, pubKey, Encoders.Hex.DecodeData(request.K1));

        var response = LNUrlStatusResponse.OkResponse();
        if (!isOkLogin)
        {
            response = LNUrlStatusResponse.InvalidSignatureResponse();
        }

        await SendMessageToHub(response, user, request.K1);
        return response;
    }

    private async Task SendMessageToHub(LNUrlStatusResponse response, AppUser? user, string k1)
    {
        var succeed = response.Status == "OK";
        string? accessToken = null;
        string? refreshToken = null;
        if (succeed)
        {
            var credentials = await TokenUtil.GenerateUserToken(user!, dbContext, userManager, authSettings);
            accessToken = credentials.AccessToken;
            refreshToken = credentials.RefreshToken;
        }

        var connectionId = ConnectionsStore.GetConnectionByKey(k1);
        if (connectionId is null)
        {
            return;
        }

        
        await hub.Clients.Client(connectionId)
            .ReceiveLightningLoginResult(new LightingLoginResponseModel
            {
                Reason = response.Reason,
                Success = succeed,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        ConnectionsStore.RemoveConnection(k1);
    }
}