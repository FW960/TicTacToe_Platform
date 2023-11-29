using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Models.UserManagementModels;

public class User : IUserModel
{
    public string Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public DateTime RegistrationDate { get; set; }
}