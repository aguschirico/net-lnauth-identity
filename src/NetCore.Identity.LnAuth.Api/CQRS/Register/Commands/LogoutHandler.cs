using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Identity;
using NetCore.Identity.LnAuth.Api.ApiModels;
using NetCore.Identity.LnAuth.Api.Database;
using NetCore.Identity.LnAuth.Api.Domain.Entities;

namespace NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;

public record LogoutUserRequest(bool? IsAuthenticated, IEnumerable<Claim> Claims) : IRequest<AppResponse<bool>>;

public class LogoutHandler : IRequestHandler<LogoutUserRequest, AppResponse<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public LogoutHandler(AppDbContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<AppResponse<bool>> Handle(LogoutUserRequest request, CancellationToken cancellationToken)
    {
        if (request.IsAuthenticated ?? false)
        {
            var userName = request.Claims.First(x => x.Type == "UserName").Value;
            var appUser = _dbContext.Users.Single(x => x.UserName == userName);
            await _userManager.UpdateSecurityStampAsync(appUser);
            return new AppResponse<bool>().SetSuccessResponse(true);
        }
        return new AppResponse<bool>().SetSuccessResponse(true);
    }
}