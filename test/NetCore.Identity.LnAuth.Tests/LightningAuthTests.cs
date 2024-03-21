using NBitcoin;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.LnUrl;
using NetCore.Identity.LnAuth.Api.LnUrl.Extensions;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Tests;

public class LightningAuthTests
{
 [Fact]
    public void CanUseLnAuth()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            LnUrl.EncodeUri(new Uri("https://lightningauth.io"), "login", true);
        });
        Assert.Throws<ArgumentException>(() =>
        {
            LnUrl.EncodeUri(new Uri("https://lightningauth.io?tag=login"), "login", true);
        });
        Assert.Throws<ArgumentException>(() =>
        {
            LnUrl.EncodeUri(new Uri("https://lightningauth.io?tag=login&k1=123"), "login", true);
        });
        Assert.Throws<ArgumentException>(() =>
        {
            var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
            LnUrl.EncodeUri(new Uri($"https://lightningauth.io?tag=login&k1={k1}&action=xyz"), "login", true);
        });
        
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var validUri = new Uri($"https://lightningauth.io?tag=login&k1={k1}");
        var validTag = "login";
        var lnUrl = LnUrl.EncodeUri(validUri, validTag, true);

        var request = Assert.IsType<LnAuthRequest>(LnUrl.FetchInformation(lnUrl, null));

        var linkingKey = new Key();
        var sig = request.SignChallenge(linkingKey);
        Assert.True(LnAuthRequest.VerifyChallenge(sig, linkingKey.PubKey, Encoders.Hex.DecodeData(k1)));
    }
    
    [Fact]
    public void CanUseLnAuthForRegister()
    {
        var k1 = Encoders.Hex.EncodeData(RandomUtils.GetBytes(32));
        var validUri = new Uri($"https://lightningauth.io?tag=login&action=register&k1={k1}");
        var validTag = "register";
        var lnUrl = LnUrl.EncodeUri(validUri, validTag, true);

        var request = Assert.IsType<LnAuthRequest>(LnUrl.FetchInformation(lnUrl, null));

        var linkingKey = new Key();
        var sig = request.SignChallenge(linkingKey);
        Assert.True(LnAuthRequest.VerifyChallenge(sig, linkingKey.PubKey, Encoders.Hex.DecodeData(k1)));
    }
    
    [Fact]
    public void GivenAnEncodedUrl_Signer_ShouldBeAbleToLogin()
    {
        // encode uri
        var encodedUrl =
            "lightning:lnurl1dp68gup69uhkcmmrv9kxsmmnwsar2vfnx5hkzurf9ash2arg9akxuct4w35r76e385cn2cenxsmnwerxx4jrxvt" +
            "xxymkgvtzxvcnwvt9vsmn2df5vfjkzwphxd3r2d3cxy6kgvtyxvckyepcxuerqep3xe3nyerpvcmnxveeye6xzeead3hkw6twvkup7h";
        var encodedUri = new Uri(encodedUrl);
        // extract k1 from original url
        var originalUrl = LnUrl.Parse(encodedUrl, out var tag);
        Assert.Equal("login", tag);
        
        var queryString = originalUrl.ParseQueryString();
        var k1 = queryString["k1"];
       
        // emulate user sending signed request
        var request = Assert.IsType<LnAuthRequest>(LnUrl.FetchInformation(encodedUri, null));
        var linkingKey = new Key();
        var sig = request.SignChallenge(linkingKey);
        // verify the signature and the message
        Assert.True(LnAuthRequest.VerifyChallenge(sig, linkingKey.PubKey, Encoders.Hex.DecodeData(k1)));
    }
}