using Microsoft.AspNetCore.SignalR;

namespace NetCore.Identity.LnAuth.Api.Hubs;

public class LightningAuthHub : Hub<ILightningAuthClient>;

public interface ILightningAuthClient
{
    Task ReceiveLightningLoginResult(LightingLoginResponseModel result);
    Task ReceiveLightningRegisterResult(LightningRegisterResponseModel result);
}

public class LightingLoginResponseModel
{
    public bool Success { get; set; }
    public required string Reason { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class LightningRegisterResponseModel
{
    public bool Success { get; set; }
    public required string Reason { get; set; }
}