using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ivy.Api.Controllers;

[ApiController]
[Route("Error")]
public class ErrorController : ControllerBase
{
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        return Problem(
            detail: exception?.Message,
            title: "An error occurred while processing your request."
        );
    }
}
