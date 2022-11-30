using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public class RollingBackups : IRollingBackups
{
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;

    public RollingBackups(IBackupDirectoryProvider backupDirectoryProvider)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
    }

    public int GetCount(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootGameSavePath(gameName, saveName, saveType);
        if (!Directory.Exists(savePath))
        {
            return 0;
        }

        return Directory.GetDirectories(savePath).Length;
    }

    public string GetMostRecentPath(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootGameSavePath(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderByDescending(x => x).First();
    }

    public void DeleteOldestSave(string gameName, string saveName, SaveType saveType)
    {
        var oldest = GetOldestPath(gameName, saveName, saveType);
        Directory.Delete(oldest, true);
    }

    private string GetOldestPath(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootGameSavePath(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderBy(x => x).First();
    }
}