namespace TicTacToe_Platform.Models.UserManagementModels;

public class LoginModel : IUserModel
{
    public string Login { get; set; }
    public string Password { get; set; }
}