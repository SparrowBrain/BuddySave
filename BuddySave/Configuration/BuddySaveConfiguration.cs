using BuddySave.Core.Models;

namespace BuddySave.Configuration;

public class BuddySaveConfiguration : IBuddySaveConfiguration
{
    public string CloudPath { get; set; }

    public Session Session { get; set; }

    public GameSave GameSave { get; set; }
}