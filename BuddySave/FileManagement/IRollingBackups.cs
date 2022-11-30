using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IRollingBackups
{
    int GetCount(string gameName, string saveName, SaveType saveType);

    string GetMostRecent(string gameName, string saveName, SaveType saveType);

    void DeleteOldest(string gameName, string saveName, SaveType saveType);
}