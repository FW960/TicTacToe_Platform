namespace TicTacToe_Platform.Models.UserManagementModels;

public interface IUserModel
{
    public string Login { get; set; }
    
    public string Password { get; set; }
}