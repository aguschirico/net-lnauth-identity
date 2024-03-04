namespace NetCore.Identity.LnAuth.Api.Domain.Entities;

public class LightningAuthLinkingKey
{
    public long Id { get; set; }
    public required string K1 { get; set; }
    public LinkingKeyTypes Type { get; set; }
}

public enum LinkingKeyTypes
{
    Register,
    Login
}