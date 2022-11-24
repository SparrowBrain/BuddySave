namespace BuddySave.Configuration;

public class BuddySaveConfiguration : IBuddySaveConfiguration
{
    public string CloudPath { get; set; }

    public string UserName { get; set; }

    public GameConfiguration Game { get; set; }
}