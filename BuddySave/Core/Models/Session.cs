namespace BuddySave.Core.Models;

public class Session(string userName, string ip, string port)
{
    public string UserName { get; init; } = userName;

    public string Ip { get; init; } = ip;

    public string Port { get; init; } = port;
}