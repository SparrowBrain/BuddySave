using BuddySave.Core.Models;

namespace BuddySave.Configuration;

public interface IBuddySaveConfiguration
{
    public string CloudPath { get; }

    public Session Session { get; }
    
    public GameSave GameSave { get; }
}