namespace BuddySave.Core.Models;

public class GameSave
{
    public string GameName { get; set; }
    
    public string SaveName { get; set; }

    public string LocalPath { get; set; }

    public string CloudPath { get; set; }
}