using System.Collections.Specialized;
using System.Net;
using NBitcoin;

namespace NetCore.Identity.LnAuth.Api.LnUrl.Extensions;

public static class UriExtensions
{
    public static NameValueCollection ParseQueryString(this Uri address)
    {
        NameValueCollection queryParameters = new NameValueCollection();
        if (string.IsNullOrWhiteSpace(address.Query))
        {
            return queryParameters;
        }
        string[] querySegments = address.Query.Split('&');
        foreach(string segment in querySegments)
        {
            string[] parts = segment.Split('=');
            if (parts.Length > 0)
            {
                string key = parts[0].Trim(new char[] { '?', ' ' });
                string val = parts[1].Trim();

                queryParameters.Add(key, val);
            }
        }

        return queryParameters;
    }
    
    public static bool IsOnion(this Uri uri)
    {
        return uri.IsAbsoluteUri && uri.DnsSafeHost.EndsWith(".onion", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLocalNetwork(this Uri server)
    {
        ArgumentNullException.ThrowIfNull(server);

        if (server.HostNameType == UriHostNameType.Dns)
            return server.Host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase) ||
                   server.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
                   server.Host.EndsWith(".lan", StringComparison.OrdinalIgnoreCase) ||
                   !server.Host.Contains('.', StringComparison.OrdinalIgnoreCase);

        if (IPAddress.TryParse(server.Host, out var ip)) return ip.IsLocal() || ip.IsRFC1918();

        return false;
    }
}