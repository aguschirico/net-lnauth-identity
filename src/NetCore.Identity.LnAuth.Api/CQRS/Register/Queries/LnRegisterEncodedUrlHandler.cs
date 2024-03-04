using MediatR;
using NBitcoin;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.ApiModels;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.Hubs;
using NetCore.Identity.LnAuth.Api.LnUrl;


namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Queries;

public class LightningRegisterEncodedUrlHandler(AppDbContext dbContext)
    : IRequestHandler<LightningRegisterEncodedUrlRequest, AppResponse<LnRegisterEncodedUrlResponse>>
{
    public async Task<AppResponse<LnRegisterEncodedUrlResponse>> Handle(LightningRegisterEncodedUrlRequest request, CancellationToken cancellationToken)
    {
        var url = request.HttpContextRequest.Scheme + "://" + request.HttpContextRequest.Host + "/api/auth/LightningRegister"; 
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var queryString = $"?k1={k1}&tag=login&action=register";
        url += queryString;
        var encodedUri = UrlEncoder.EncodeUri(new Uri(url), null!, true);
        dbContext.LinkingKeys.Add(new LightningAuthLinkingKey
        {
            Type = LinkingKeyTypes.Register,
            K1 = k1
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new LnRegisterEncodedUrlResponse(encodedUri.AbsoluteUri);

        ConnectionsStore.SetConnection(k1, request.ConnectionId);
        return new AppResponse<LnRegisterEncodedUrlResponse>().SetSuccessResponse(response);
    }
}

public record LightningRegisterEncodedUrlRequest(HttpRequest HttpContextRequest, string ConnectionId)
    : IRequest<AppResponse<LnRegisterEncodedUrlResponse>>;


public record LnRegisterEncodedUrlResponse(string EncodedUrl);