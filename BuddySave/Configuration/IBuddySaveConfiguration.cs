using BuddySave.Core.Models;

namespace BuddySave.Configuration;

public interface IBuddySaveConfiguration
{
    public string CloudPath { get; }

    public Session Session { get; }

    ServerParameters ServerParameters { get; set; }

    public GameSave GameSave { get; }
}