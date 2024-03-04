namespace NetCore.Identity.LnAuth.Api.Configuration;

public class AuthSettings
{
    public static string SectionName => "Auth";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public int TokenExpireSeconds { get; set; }
    public int RefreshTokenExpireSeconds { get; set; }
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public static string RefreshTokenProviderName => "REFRESHTOKENPROVIDER";
}