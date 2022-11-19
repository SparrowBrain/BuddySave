namespace BuddySave;

public class GameSave
{
    public string Name { get; set; }

    public string LocalPath { get; set; }

    public string CloudPath { get; set; }

    public GameSave(string name, string localPath, string cloudPath)
    {
        Name = name;
        LocalPath = localPath;
        CloudPath = cloudPath;
    }
}