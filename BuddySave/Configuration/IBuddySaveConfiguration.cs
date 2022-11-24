namespace BuddySave.Configuration;

public interface IBuddySaveConfiguration
{
    public string CloudPath { get; }

    public string UserName { get; }
    
    public GameConfiguration Game { get; }
}