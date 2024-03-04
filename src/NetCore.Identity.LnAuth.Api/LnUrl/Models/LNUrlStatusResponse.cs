using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCore.Identity.LnAuth.Api.LnUrl.Models;

public class LNUrlStatusResponse
{
    [JsonProperty("status")] public string Status { get; set; } = null!;
    [JsonProperty("reason")] public string Reason { get; set; } = null!;

    public static bool IsErrorResponse(JObject response, out LNUrlStatusResponse? status)
    {
        if (response.ContainsKey("status") && response["status"]!.Value<string>()!
                .Equals("Error", StringComparison.OrdinalIgnoreCase))
        {
            status = response.ToObject<LNUrlStatusResponse>()!;
            return true;
        }

        status = null;
        return false;
    }

    public static LNUrlStatusResponse OkResponse()
    {
        return new LNUrlStatusResponse()
        {
            Status = "OK"
        };
    }

    public static LNUrlStatusResponse InvalidSignatureResponse()
    {
        return new LNUrlStatusResponse()
        {
            Reason = "Invalid Signature",
            Status = "ERROR"
        };
    }

    public static LNUrlStatusResponse UnknownActionResponse()
    {
        return new LNUrlStatusResponse
        {
            Reason = "Invalid or Unknown Action", Status = "ERROR"
        };
    }

    public static LNUrlStatusResponse InvalidK1()
    {
        return new LNUrlStatusResponse
        {
            Reason = "Invalid LinkingKey", Status = "ERROR"
        };
    }

    public static LNUrlStatusResponse UserAlreadyExistsResponse()
    {
        return new LNUrlStatusResponse
        {
            Reason = "User already registered", Status = "ERROR"
        };    
    }

    public static LNUrlStatusResponse UserNotFound()
    {
        return new LNUrlStatusResponse
        {
            Reason = "User not found", Status = "ERROR"
        };
    }
}