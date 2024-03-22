using Microsoft.AspNetCore.Identity;

namespace NetCore.Identity.LnAuth.Api.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string? PubKey { get; set; }
}