namespace TicTacToe_Platform.Models.Configurations;

public class AuthorizationConfiguration : IMyConfiguration
{
    public int TokenLiveTimeSeconds { get; set; }
    public static TimeSpan TokenLiveTime { get; set; }

    public string Key { get; set; }
    public int IvSize { get; set; }
    

}