using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Views.Pages;

namespace TicTacToe_Platform.Controllers.RenderControllers;

[Route("")]
public class LoginSignUpController : Controller
{
    [HttpGet("SignUp")]
    [HttpGet("Login")]
    public IActionResult Login()
    {
        var model = new SignUpLoginModel
        {
            IsLogin = HttpContext.Request.Path.Value.Contains("login", StringComparison.OrdinalIgnoreCase)
        };
        
        return View("/Views/Pages/SignUpLoginPage.cshtml", model);
    }
}