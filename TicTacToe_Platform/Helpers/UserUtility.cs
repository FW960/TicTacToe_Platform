using System.Diagnostics;
using Dapper;
using MySql.Data.MySqlClient;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Helpers;

public class UserUtility
{
    private readonly MySqlConnection _sqlConnection;
    private readonly CryptoUtility _cryptoUtility;

    public UserUtility(MySqlConnection sqlConnection, CryptoUtility cryptoUtility)
    {
        _sqlConnection = sqlConnection;
        _cryptoUtility = cryptoUtility;
    }

    public bool TryFindUserById(string userId, out User foundUser)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                foundUser = null;
                return false;
            }

            foundUser = _sqlConnection.Query<User>($"SELECT * FROM USERS WHERE Id='{userId}'").FirstOrDefault();

            return foundUser is not null;
        }
        catch (Exception e)
        {
            //todo logger
            Debug.WriteLine(e);
            throw;
        }
    }

    public bool TryFindUser(out User foundUser, string userName, string? password = null)
    {
        try
        {
            if (string.IsNullOrEmpty(userName))
            {
                foundUser = null;
                return false;
            }

            foundUser = _sqlConnection
                .Query<User>(
                    $"SELECT * FROM USERS WHERE Login = '{userName}' {(string.IsNullOrEmpty(password) ? "" : $"AND Password = '{password}'")}")
                .FirstOrDefault();

            return foundUser is not null;
        }
        catch (Exception e)
        {
            //todo logger
            Debug.WriteLine(e);
            throw;
        }
    }

    public bool CreateUser(SignUpModel model, out User? user)
    {
        try
        {
            if (model is null)
            {
                user = null;
                return false;
            }

            var encryptedUser = _cryptoUtility.ShaMacEncryptUserData(model);

            var cmd = _sqlConnection.CreateCommand();
            
            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Login = encryptedUser.Login,
                Password = encryptedUser.Password,
                RegistrationDate = DateTime.UtcNow
            };

            cmd.CommandText =
                $"INSERT INTO Users (Id, Login, Password, RegistrationDate) VALUES ('{user.Id}', '{encryptedUser.Login}', '{encryptedUser.Password}', '{user.RegistrationDate:yyyy-MM-dd H:mm:ss}');";

            var result = cmd.ExecuteNonQuery();

            return result > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Debug.WriteLine(e);
            throw;
        }

    }
}