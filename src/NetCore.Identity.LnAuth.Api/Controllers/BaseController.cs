using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace NetCore.Identity.LnAuth.Api.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected ISender Mediator { get; }

    public BaseController(ISender mediator)
    {
        Mediator = mediator;
    }
}