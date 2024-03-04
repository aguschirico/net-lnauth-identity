using System.Text;
using NetCore.Identity.LnAuth.Api.LnUrl.Extensions;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Api.LnUrl;

public class UrlEncoder
{
    private static readonly Dictionary<string, string> SchemeTagMapping =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            {"lnurlc", "channelRequest"},
            {"lnurlw", "withdrawRequest"},
            {"lnurlp", "payRequest"},
            {"keyauth", "login"}
        };

    private static readonly Dictionary<string, string> SchemeTagMappingReversed =
        SchemeTagMapping.ToDictionary(pair => pair.Value, pair => pair.Key,
            StringComparer.InvariantCultureIgnoreCase);
    
    public static Uri EncodeUri(Uri serviceUrl, string tag, bool bech32)
    {
        if (serviceUrl.Scheme != "https" && !serviceUrl.IsOnion() && !serviceUrl.IsLocalNetwork())
            throw new ArgumentException("serviceUrl must be an onion service OR https based OR on the local network",
                nameof(serviceUrl));
        if (string.IsNullOrEmpty(tag)) tag = serviceUrl.ParseQueryString().Get("tag")!;
        if (tag == "login") LnAuthRequest.EnsureValidUrl(serviceUrl);
        if (bech32) return new Uri($"lightning:{EncodeBech32(serviceUrl)}");

        if (string.IsNullOrEmpty(tag)) tag = serviceUrl.ParseQueryString().Get("tag")!;

        if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag), "tag must be provided");

        if (!SchemeTagMappingReversed.TryGetValue(tag.ToLowerInvariant(), out var scheme))
            throw new ArgumentOutOfRangeException(nameof(tag), $"tag must be either {string.Join(',', SchemeTagMappingReversed.Select(pair => pair.Key))}");


        return new UriBuilder(serviceUrl)
        {
            Scheme = scheme
        }.Uri;
    }
    public static string EncodeBech32(Uri serviceUrl)
    {
        if (serviceUrl.Scheme != "https" && !serviceUrl.IsOnion() && !serviceUrl.IsLocalNetwork())
            throw new ArgumentException("serviceUrl must be an onion service OR https based OR on the local network",
                nameof(serviceUrl));

        return Bech32Engine.Encode("lnurl", Encoding.UTF8.GetBytes(serviceUrl.ToString()))!;
    }
    
}