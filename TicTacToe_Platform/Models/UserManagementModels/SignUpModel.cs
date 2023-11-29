namespace TicTacToe_Platform.Models.UserManagementModels;

public class SignUpModel : IUserModel
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string PasswordConfirm { get; set; }
}