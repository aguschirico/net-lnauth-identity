using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NetCore.Identity.LnAuth.Api.Configuration;
using NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;

namespace NetCore.Identity.LnAuth.Api.CQRS;



public static class TokenUtil
{
    public static async Task<LoginUserResponse> GenerateUserToken(AppUser user, AppDbContext dbContext,
        UserManager<AppUser> userManager, AuthSettings authSettings)
    {
        var claims = (from ur in dbContext.UserRoles
                where ur.UserId == user.Id
                join r in dbContext.Roles on ur.RoleId equals r.Id
                join rc in dbContext.RoleClaims on r.Id equals rc.RoleId
                select rc)
            .Where(rc => rc.ClaimValue != null && rc.ClaimType != null)
            .Select(rc => new Claim(rc.ClaimType ?? "", rc.ClaimValue ?? ""))
            .Distinct()
            .ToList();
        var token = TokenUtil.GetJwtToken(authSettings, user, claims);
        await userManager.RemoveAuthenticationTokenAsync(user, AuthSettings.RefreshTokenProviderName, "RefreshToken");
        var refreshToken = await userManager
            .GenerateUserTokenAsync(user, AuthSettings.RefreshTokenProviderName, "RefreshToken");
        await userManager
            .SetAuthenticationTokenAsync(user, AuthSettings.RefreshTokenProviderName, "RefreshToken", refreshToken);
        return new LoginUserResponse(token, refreshToken);
    }

    public static string GetJwtToken(AuthSettings appSettings, AppUser user, IEnumerable<Claim> roleClaims)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.SecretKey));
        var signInCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var userClaims = new List<Claim>
        {
            new("Id", user.Id.ToString()),
            new("UserName", user.UserName ?? "")
        };
        userClaims.AddRange(roleClaims);
        var tokeOptions = new JwtSecurityToken(
            issuer: appSettings.Issuer,
            audience: appSettings.Audience,
            claims: userClaims,
            expires: DateTime.UtcNow.AddSeconds(appSettings.TokenExpireSeconds),
            signingCredentials: signInCredentials
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        return tokenString;
    }

    public static ClaimsPrincipal GetPrincipalFromExpiredToken(AuthSettings appSettings, string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = appSettings.Audience,
            ValidIssuer = appSettings.Issuer,
            ValidateLifetime = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.SecretKey))
        };

        var principal =
            new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters,
                out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg
                .Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            throw new SecurityTokenException("GetPrincipalFromExpiredToken Token is not validated");

        return principal;
    }
}