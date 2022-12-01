using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IRollingBackups
{
    string GetMostRecent(string gameName, string saveName, SaveType saveType);

    void Add(string sourcePath, string gameName, string saveName, SaveType saveType);
}