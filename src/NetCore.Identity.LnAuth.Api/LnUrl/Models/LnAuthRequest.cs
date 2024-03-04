using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NetCore.Identity.LnAuth.Api.LnUrl.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace NetCore.Identity.LnAuth.Api.LnUrl.Models;

public class LnAuthRequest
{
    public enum LnAuthRequestAction
    {
        Register,
        Login,
        Link,
        Auth
    }

    public Uri LightningUrl { get; set; } = null!;

    [JsonProperty("tag")] public static string Tag => "login";
    [JsonProperty("k1")] public string? K1 { get; set; }

    [JsonProperty("action")]
    [JsonConverter(typeof(StringEnumConverter))]
    public LnAuthRequestAction? Action { get; set; }

    public async Task<LNUrlStatusResponse> SendChallenge(ECDSASignature sig, PubKey key, HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        var url = LightningUrl;
        var uriBuilder = new UriBuilder(url);
        LnUrl.AppendPayloadToQuery(uriBuilder, "sig", Encoders.Hex.EncodeData(sig.ToDER()));
        LnUrl.AppendPayloadToQuery(uriBuilder, "key", key.ToHex());
        url = new Uri(uriBuilder.ToString());
        var response = await httpClient.GetAsync(url, cancellationToken);
        var json = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        return json.ToObject<LNUrlStatusResponse>()!;
    }

    public Task<LNUrlStatusResponse> SendChallenge(Key key, HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        var sig = SignChallenge(key);
        return SendChallenge(sig, key.PubKey, httpClient, cancellationToken);
    }

    public ECDSASignature SignChallenge(Key key)
    {
        return SignChallenge(key, K1!);
    }

    public static ECDSASignature SignChallenge(Key key, string k1)
    {
        var messageBytes = Encoders.Hex.DecodeData(k1);
        var messageHash = new uint256(messageBytes);
        return key.Sign(messageHash);
    }

    public static void EnsureValidUrl(Uri serviceUrl)
    {
        var tag = serviceUrl.ParseQueryString().Get("tag");
        if (tag != "login")
            throw new ArgumentException("LNURL-Auth(LUD04) requires tag to be provided straight away",
                nameof(serviceUrl));
        var k1 = serviceUrl.ParseQueryString().Get("k1");
        if (k1 is null)
            throw new ArgumentException("LNURL-Auth(LUD04) requires k1 to be provided", nameof(serviceUrl));

        byte[] k1Bytes;
        try
        {
            k1Bytes = Encoders.Hex.DecodeData(k1);
        }
        catch (Exception)
        {
            throw new ArgumentException("LNURL-Auth(LUD04) requires k1 to be hex encoded", nameof(serviceUrl));
        }

        if (k1Bytes.Length != 32)
            throw new ArgumentException("LNURL-Auth(LUD04) requires k1 to be 32bytes", nameof(serviceUrl));

        var action = serviceUrl.ParseQueryString().Get("action");
        if (action != null && !Enum.TryParse(typeof(LnAuthRequestAction), action, true, out _))
            throw new ArgumentException("LNURL-Auth(LUD04) action value was invalid", nameof(serviceUrl));
    }

    public static bool VerifyChallenge(ECDSASignature sig, PubKey expectedPubKey, byte[] expectedMessage)
    {
        var messageHash = new uint256(expectedMessage);
        return expectedPubKey.Verify(messageHash, sig);
    }
}