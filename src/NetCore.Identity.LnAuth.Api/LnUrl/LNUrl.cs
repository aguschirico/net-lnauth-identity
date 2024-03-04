using System.Globalization;
using System.Net;
using System.Text;
using NetCore.Identity.LnAuth.Api.LnUrl.Extensions;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Api.LnUrl;

public class LnUrl
{
    private static readonly Dictionary<string, string> SchemeTagMapping =
        new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "lnurlc", "channelRequest" },
            { "lnurlw", "withdrawRequest" },
            { "lnurlp", "payRequest" },
            { "keyauth", "login" }
        };

    private static readonly Dictionary<string, string> SchemeTagMappingReversed =
        SchemeTagMapping.ToDictionary(pair => pair.Value, pair => pair.Key,
            StringComparer.InvariantCultureIgnoreCase);

    public static void AppendPayloadToQuery(UriBuilder uri, string key, string value)
    {
        if (uri.Query.Length > 1)
            uri.Query += "&";

        uri.Query = uri.Query + WebUtility.UrlEncode(key) + "=" +
                    WebUtility.UrlEncode(value);
    }

    public static Uri Parse(string lnurl, out string? tag)
    {
        lnurl = lnurl.Replace("lightning:", "", StringComparison.InvariantCultureIgnoreCase);
        if (lnurl.StartsWith("lnurl1", StringComparison.InvariantCultureIgnoreCase))
        {
            Bech32Engine.Decode(lnurl, out _, out var data);
            var result = new Uri(Encoding.UTF8.GetString(data!));

            if (!result.IsOnion() && !result.Scheme.Equals("https", StringComparison.Ordinal) && !result.IsLocalNetwork())
                throw new FormatException("LNURL provided is not secure.");

            var query = result.ParseQueryString();
            tag = query.Get("tag");
            return result;
        }

        if (Uri.TryCreate(lnurl, UriKind.Absolute, out var lud17Uri) &&
            SchemeTagMapping.TryGetValue(lud17Uri.Scheme.ToLowerInvariant(), out tag))
            return new Uri(lud17Uri.ToString()
                .Replace(lud17Uri.Scheme + ":", lud17Uri.IsOnion() ? "http:" : "https:"));

        throw new FormatException("LNURL uses bech32 and 'lnurl' as the hrp (LUD1) or an lnurl LUD17 scheme. ");
    }
    
    public static string EncodeBech32(Uri serviceUrl)
    {
        if (serviceUrl.Scheme != "https" && !serviceUrl.IsOnion() && !serviceUrl.IsLocalNetwork())
            throw new ArgumentException("serviceUrl must be an onion service OR https based OR on the local network",
                nameof(serviceUrl));

        return Bech32Engine.Encode("lnurl", Encoding.UTF8.GetBytes(serviceUrl.ToString()))!;
    }

    public static Uri EncodeUri(Uri serviceUrl, string? tag, bool bech32)
    {
        if (serviceUrl.Scheme != "https" && !serviceUrl.IsOnion() && !serviceUrl.IsLocalNetwork())
            throw new ArgumentException("serviceUrl must be an onion service OR https based OR on the local network",
                nameof(serviceUrl));
        if (string.IsNullOrEmpty(tag)) tag = serviceUrl.ParseQueryString().Get("tag");
        if (tag == "login") LnAuthRequest.EnsureValidUrl(serviceUrl);
        if (bech32) return new Uri($"lightning:{EncodeBech32(serviceUrl)}");

        if (string.IsNullOrEmpty(tag)) tag = serviceUrl.ParseQueryString().Get("tag");

        if (string.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag), "tag must be provided");

        if (!SchemeTagMappingReversed.TryGetValue(tag.ToLowerInvariant(), out var scheme))
            throw new ArgumentOutOfRangeException(nameof(tag),
                $"tag must be either {string.Join(',', SchemeTagMappingReversed.Select(pair => pair.Key))}");


        return new UriBuilder(serviceUrl)
        {
            Scheme = scheme
        }.Uri;
    }

    public static Uri ExtractUriFromInternetIdentifier(string identifier)
    {
        var s = identifier.Split("@");
        var s2 = s[1].Split(":");
        UriBuilder uriBuilder;
        if (s2.Length > 1)
        {
            var scheme = s[1].EndsWith(".onion", StringComparison.InvariantCultureIgnoreCase) ? "http" : "https";
            var portNumber = int.Parse(s2[1], NumberFormatInfo.InvariantInfo);

            uriBuilder = new UriBuilder(scheme, s2[0], portNumber)
            {
                Path = $"/.well-known/lnurlp/{s[0]}"
            };
        }
        else
            uriBuilder =
                new UriBuilder(s[1].EndsWith(".onion", StringComparison.InvariantCultureIgnoreCase) ? "http" : "https",
                    s2[0])
                {
                    Path = $"/.well-known/lnurlp/{s[0]}"
                };

        return uriBuilder.Uri;
    }

    public static object FetchInformation(Uri lnUrl, string? tag)
    {
        try
        {
            lnUrl = Parse(lnUrl.ToString(), out tag);
        }
        catch (Exception)
        {
            // ignored
        }

        if (tag is null)
        {
            tag = lnUrl.ParseQueryString().Get("tag");
        }

        if (tag != "login")
        {
            throw new LnUrlException($"Cannot use the specified tag: {tag}. This is an only authentication service");
        }

        var queryString = lnUrl.ParseQueryString();
        var k1 = queryString.Get("k1");
        var action = queryString.Get("action");

        return new LnAuthRequest
        {
            K1 = k1,
            LightningUrl = lnUrl,
            Action = string.IsNullOrEmpty(action)
                ? null
                : Enum.Parse<LnAuthRequest.LnAuthRequestAction>(action, true)
        };
    }
}