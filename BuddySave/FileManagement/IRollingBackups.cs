using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IRollingBackups
{
    int GetCount(string gameName, string saveName, SaveType saveType);

    string GetMostRecentPath(string gameName, string saveName, SaveType saveType);

    void DeleteOldestSave(string gameName, string saveName, SaveType saveType);
}