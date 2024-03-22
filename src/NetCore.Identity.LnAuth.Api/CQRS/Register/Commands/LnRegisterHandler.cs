using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.Hubs;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;

public record LightningRegisterRequest(string Sig, string Key, string K1, string Action)
    : IRequest<LNUrlStatusResponse>;

public class LnRegisterHandler(
    AppDbContext dbContext,
    UserManager<AppUser> userManager,
    IHubContext<LightningAuthHub, ILightningAuthClient> hub)
    : IRequestHandler<LightningRegisterRequest, LNUrlStatusResponse>
{
    public async Task<LNUrlStatusResponse> Handle(
        LightningRegisterRequest request, CancellationToken cancellationToken)
    {
        var action = Enum.Parse<LnAuthRequest.LnAuthRequestAction>(request.Action, true);
        return action switch
        {
            LnAuthRequest.LnAuthRequestAction.Register => await TryToRegister(request, cancellationToken),
            _ => LNUrlStatusResponse.UnknownActionResponse()
        };
    }

    private async Task<LNUrlStatusResponse> TryToRegister(LightningRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var isK1Valid =
            await dbContext.LinkingKeys
                .AnyAsync(x =>
                    x.Type == LinkingKeyTypes.Register && x.K1 == request.K1, cancellationToken);

        if (!isK1Valid)
        {
            return LNUrlStatusResponse.InvalidK1();
        }

        var key = new PubKey(request.Key);
        var signature = new ECDSASignature(Encoders.Hex.DecodeData(request.Sig));
        var isOkRegister = LnAuthRequest.VerifyChallenge(signature, key, Encoders.Hex.DecodeData(request.K1));

        if (!isOkRegister)
        {
            return LNUrlStatusResponse.InvalidSignatureResponse();
        }

        var userExists = await userManager.Users
            .AnyAsync(x => x.PubKey == request.Key, cancellationToken);
        if (userExists)
        {
            return LNUrlStatusResponse.UserAlreadyExistsResponse();       
        }

        var username = $"anonymous-{RandomUtils.GetInt32()}";
        var user = new AppUser()
        {
            PubKey = request.Key,
            UserName = username
        };

        await userManager.CreateAsync(user);

        await dbContext.LinkingKeys.Where(x => x.K1 == request.K1)
            .ExecuteDeleteAsync(cancellationToken);

        await SendMessageToHub(LNUrlStatusResponse.OkResponse(), request.K1);
        return LNUrlStatusResponse.OkResponse();
    }

    private async Task SendMessageToHub(LNUrlStatusResponse response, string k1)
    {
        var succeed = response.Status == "OK";
        
        var connectionId = ConnectionsStore.GetConnectionByKey(k1);
        if (connectionId is null)
        {
            return;
        }

        await hub.Clients.Client(connectionId).ReceiveLightningRegisterResult(
            new LightningRegisterResponseModel()
            {
                Reason = response.Reason,
                Success = succeed
            });
        ConnectionsStore.RemoveConnection(k1);
    }
}