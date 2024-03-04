using MediatR;
using NBitcoin;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.ApiModels;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.Hubs;

namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Queries;

public record LightningLoginEncodedUrlRequest(HttpRequest HttpContextRequest, string ConnectionId)
    : IRequest<AppResponse<LnEncodedUrlResponse>>;

public record LnEncodedUrlResponse(string EncodedUrl);

public class LnLoginEncodedUrlHandler(AppDbContext dbContext) : IRequestHandler<LightningLoginEncodedUrlRequest, AppResponse<LnEncodedUrlResponse>>
{
    public async Task<AppResponse<LnEncodedUrlResponse>> Handle(LightningLoginEncodedUrlRequest request, CancellationToken cancellationToken)
    {
        var url = request.HttpContextRequest.Scheme + "://" + request.HttpContextRequest.Host + "/api/auth/LightningLogin"; 
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var queryString = $"?k1={k1}&tag=login&action=login";
        url += queryString;

        var encodedUri = LnUrl.UrlEncoder.EncodeUri(new Uri(url), null, true);
        dbContext.LinkingKeys.Add(new LightningAuthLinkingKey
        {
            Type = LinkingKeyTypes.Login,
            K1 = k1
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var response = new LnEncodedUrlResponse(encodedUri.AbsoluteUri);
        ConnectionsStore.SetConnection(k1, request.ConnectionId);
        return new AppResponse<LnEncodedUrlResponse>().SetSuccessResponse(response);
    }
}
