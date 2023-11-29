using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Controllers.ApiControllers;

[Route("Api/Users")]
public class ApiUserManagerController : Controller
{
    private readonly UserUtility _userUtility;
    private readonly IdentityUtility _identityUtility;
    private readonly DataValidator _dataValidator;
    private readonly CryptoUtility _cryptoUtility;


    public ApiUserManagerController(UserUtility userUtility, IdentityUtility identityUtility, DataValidator dataValidator, CryptoUtility cryptoUtility)
    {
        _userUtility = userUtility;
        _identityUtility = identityUtility;
        _dataValidator = dataValidator;
        _cryptoUtility = cryptoUtility;
    }
    
    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        
        if (!_dataValidator.ValidateLoginModel(model, out var error))
        {
            return BadRequest(error);
        }
        
        if (_userUtility.TryFindUser(out var user, _cryptoUtility.ShaMacEncryptString(model.Login), _cryptoUtility.ShaMacEncryptString(model.Password)))
        {
            _identityUtility.Login(HttpContext, user);
            
            return Ok();
        }

        return BadRequest("Incorrect login data");

    }
    
    [HttpPost("SignUp")]
    public IActionResult SignUp([FromBody] SignUpModel model)
    {
        if (!_dataValidator.ValidateSignUpModel(model, out var error))
        {
            return BadRequest(error);
        }

        if (_userUtility.TryFindUser(out var user, _cryptoUtility.ShaMacEncryptString(model.Login)))
        {
            return BadRequest("User already exists");
        }
        
        if (_userUtility.CreateUser(model, out user))
        {
            _identityUtility.Login(HttpContext, user!);
            
            return Ok();
        }

        return BadRequest("Error trying to create user");
    }

    [HttpGet("Logout")]
    public IActionResult LogOut()
    {
        _identityUtility.LogOut(HttpContext);
        return Redirect("/login");
    }
}