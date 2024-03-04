using MediatR;
using Microsoft.AspNetCore.Identity;
using NetCore.Identity.LnAuth.Api.ApiModels;
using NetCore.Identity.LnAuth.Api.Configuration;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;

namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;

public record RefreshTokenRequest
    (string AccessToken, string RefreshToken) : IRequest<AppResponse<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken);

public class RefreshTokenHandler : IRequestHandler<RefreshTokenRequest, AppResponse<RefreshTokenResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly AuthSettings _authSettings;
    private readonly UserManager<AppUser> _userManager;

    public RefreshTokenHandler(AppDbContext dbContext, AuthSettings authSettings, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _authSettings = authSettings;
        _userManager = userManager;
    }

    public async Task<AppResponse<RefreshTokenResponse>> Handle(RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var principal = TokenUtil.GetPrincipalFromExpiredToken(_authSettings, request.AccessToken);
        if (principal.FindFirst("UserName")?.Value == null)
        {
            return new AppResponse<RefreshTokenResponse>().SetErrorResponse("email", "User not found");
        }
        else
        {
            var user = await _userManager.FindByNameAsync(principal.FindFirst("UserName")?.Value ?? "");
            if (user == null)
            {
                return new AppResponse<RefreshTokenResponse>().SetErrorResponse("email", "User not found");
            }

            if (!await _userManager.VerifyUserTokenAsync(user, AuthSettings.RefreshTokenProviderName, "RefreshToken",
                    request.RefreshToken))
            {
                return new AppResponse<RefreshTokenResponse>().SetErrorResponse("token", "Refresh token expired");
            }

            var token = await TokenUtil.GenerateUserToken(user, _dbContext, _userManager, _authSettings);
            return new AppResponse<RefreshTokenResponse>()
                .SetSuccessResponse(new RefreshTokenResponse(token.AccessToken,token.RefreshToken));
        }
    }
}