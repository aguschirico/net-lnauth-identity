using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using NetCore.Identity.LnAuth.Api.LnUrl;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;
using NetCore.Identity.LnAuth.Tests.Plumbing;
using Newtonsoft.Json;

namespace NetCore.Identity.LnAuth.Tests;

public class IntegrationTests(IntegrationTestsFactory<Program, AppDbContext> factory)
    : IClassFixture<IntegrationTestsFactory<Program, AppDbContext>>
{
    [Fact]
    public async Task CallingLnAuth_WithValidSignature_ShouldReturnValidResult()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var httpClient = factory.CreateClient();
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var queryString = $"?k1={k1}&tag=login&action=register";
        var url = "http://localhost";
        url += queryString;
        var encodedUri = LnUrl.EncodeUri(new Uri(url), null, true);
        dbContext.LinkingKeys.Add(new LightningAuthLinkingKey
        {
            Type = LinkingKeyTypes.Register,
            K1 = k1
        });
        await dbContext.SaveChangesAsync();
        
        var request = Assert.IsType<LnAuthRequest>(LnUrl.FetchInformation(encodedUri, null));

        var linkingKey = new Key();
        var sig = request.SignChallenge(linkingKey);

        queryString += $"&sig={Convert.ToHexString(sig.ToDER())}&key={linkingKey.PubKey.ToHex()}";
        var response = await httpClient.GetAsync($"api/auth/lightning-register{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var lnUrlStatusResponse = JsonConvert.DeserializeObject<LNUrlStatusResponse>(responseString);
        Assert.True(LnAuthRequest.VerifyChallenge(sig, linkingKey.PubKey, Encoders.Hex.DecodeData(k1)));
        Assert.NotNull(lnUrlStatusResponse);
        Assert.Equal("OK",lnUrlStatusResponse.Status);

        await dbContext.SaveChangesAsync();

        await TestLogin(linkingKey, dbContext, httpClient);
    }

    private static async Task TestLogin(Key linkingKey, AppDbContext dbContext, HttpClient httpClient)
    {
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var queryString = $"?k1={k1}&tag=login&action=login";
        var url = "http://localhost";
        url += queryString;
        var encodedUri = LnUrl.EncodeUri(new Uri(url), null, true);
        dbContext.LinkingKeys.Add(new LightningAuthLinkingKey
        {
            Type = LinkingKeyTypes.Login,
            K1 = k1
        });
        await dbContext.SaveChangesAsync();
        
        var request = Assert.IsType<LnAuthRequest>(LnUrl.FetchInformation(encodedUri, null));
        var sig = request.SignChallenge(linkingKey);

        queryString += $"&sig={Convert.ToHexString(sig.ToDER())}&key={linkingKey.PubKey.ToHex()}";
        var response = await httpClient.GetAsync($"api/auth/lightning-login{queryString}");
        var stringResponse = await response.Content.ReadAsStringAsync();
        var lnUrlStatusResponse = JsonConvert.DeserializeObject<LNUrlStatusResponse>(stringResponse);
        Assert.True(LnAuthRequest.VerifyChallenge(sig, linkingKey.PubKey, Encoders.Hex.DecodeData(k1)));
        Assert.NotNull(lnUrlStatusResponse);
        Assert.Equal("OK",lnUrlStatusResponse.Status);
    }
}