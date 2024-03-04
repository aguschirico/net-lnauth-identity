namespace NetCore.Identity.LnAuth.Api.Hubs;


public static class ConnectionsStore
{
    private static Dictionary<string, string> GroupedConnections { get; } = new();

    public static void SetConnection(string key, string value)
    {
        GroupedConnections[key] = value;
    }

    public static string? GetConnectionByKey(string key)
    {
        GroupedConnections.TryGetValue(key, out var value);
        return value;
    }

    public static void RemoveConnection(string key)
    {
        GroupedConnections.Remove(key);
    }
}