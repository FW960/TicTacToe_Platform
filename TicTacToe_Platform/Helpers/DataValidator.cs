using System.Text.RegularExpressions;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Helpers;

public class DataValidator
{
    private readonly CryptoUtility _cryptoUtility;
    private readonly UserUtility _userUtility;

    public DataValidator(CryptoUtility cryptoUtility, UserUtility userUtility)
    {
        _cryptoUtility = cryptoUtility;
        _userUtility = userUtility;
    }
    
    public bool ValidateSignUpModel(SignUpModel model, out string error)
    {
        if (SqlInjectionValidator(model.Login) || SqlInjectionValidator(model.Password) ||
            SqlInjectionValidator(model.PasswordConfirm))
        {
            error = "Sql injection attempt";
            return false;
        }
        
        if (!model.Password.Equals(model.PasswordConfirm))
        {
            error = "Passwords doesn't match";
            return false;
        }

        if (_userUtility.TryFindUser(out var user, _cryptoUtility.ShaMacEncryptString(model.Login)))
        {
            error = "User already exists";
            return false;
        }
        

        error = "";
        return true;
    }

    public bool ValidateLoginModel(LoginModel model, out string error)
    {
        if (SqlInjectionValidator(model.Login) || SqlInjectionValidator(model.Password))
        {
            error = "Sql injection attempt";
            return false;
        }

        error = "";

        return true;
    }

    private bool SqlInjectionValidator(string input)
    {
        // Regular expression pattern to detect common SQL injection keywords
        string sqlInjectionPattern =
            @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT( +INTO)?|MERGE|SELECT|UPDATE)\b)|(\b(AND|OR)\b\s*[\^=\-\d\s()])";

        // Case-insensitive regex match
        var regex = new Regex(sqlInjectionPattern, RegexOptions.IgnoreCase);

        // Check if the input matches the SQL injection pattern
        return regex.IsMatch(input);
    }
}