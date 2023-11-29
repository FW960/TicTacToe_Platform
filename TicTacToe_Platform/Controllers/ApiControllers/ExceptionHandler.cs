using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TicTacToe_Platform.Controllers.ApiControllers;

[Route("Exception")]
public class ExceptionHandler : Controller
{
    [Route("Handler")]
    public IActionResult Index()
    {
        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandler is null)
            return NotFound();

        var error = exceptionHandler.Error;
        //todo log
        return Ok();
    }

}