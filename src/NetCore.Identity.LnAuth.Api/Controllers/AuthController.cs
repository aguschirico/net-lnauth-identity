using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetCore.Identity.LnAuth.Api.ApiModels;
using NetCore.Identity.LnAuth.Api.CQRS.Register.Commands;
using NetCore.Identity.LnAuth.Api.CQRS.Register.Queries;
using NetCore.Identity.LnAuth.Api.LnUrl.Models;

namespace NetCore.Identity.LnAuth.Api.Controllers;

[ApiController]
public class AuthController(ISender mediator) : BaseController(mediator)
{
    
    [HttpGet]
    public async Task<AppResponse<LnRegisterEncodedUrlResponse>> EncodedRegisterUrl([FromQuery] string connectionId)
    {
        return await Mediator.Send(new LightningRegisterEncodedUrlRequest(HttpContext.Request, connectionId));
    }

    [HttpGet]
    public async Task<LNUrlStatusResponse> LightningRegister([FromQuery] LightningRegisterRequest req)
    {
        return await Mediator.Send(req);
    }
    
    [HttpGet]
    public async Task<AppResponse<LnEncodedUrlResponse>> EncodedLoginUrl([FromQuery] string connectionId)
    {
        return await Mediator.Send(new LightningLoginEncodedUrlRequest(HttpContext.Request, connectionId));
    }

    [HttpGet]
    public async Task<LNUrlStatusResponse> LightningLogin([FromQuery] LightningLoginRequest req)
    {
        return await Mediator.Send(req);
    }

    [HttpPost]
    public async Task<AppResponse<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest req)
    {
        return await Mediator.Send(req);
    }
    
    [HttpPost]
    public async Task<AppResponse<bool>> Logout()
    {
        return await Mediator.Send(new LogoutUserRequest(User.Identity?.IsAuthenticated, User.Claims));
    }
}