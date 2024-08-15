using BuddySave.Core.Models;
using Microsoft.Extensions.Logging;

namespace BuddySave.FileManagement;

public class RollingBackups(
    ILogger<RollingBackups> logger, 
    IBackupDirectoryProvider backupDirectoryProvider, 
    ISaveCopier saveCopier)
    : IRollingBackups
{
    private const int MaxNumberOfRollingBackups = 10;

    public string GetMostRecent(string gameName, string saveName, SaveType saveType)
    {
        var savePath = backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderByDescending(x => x).First();
    }

    public void Add(string sourcePath, string gameName, string saveName, SaveType saveType)
    {
        var backupDir = backupDirectoryProvider.GetTimestampedDirectory(gameName, saveName, saveType);
        saveCopier.CopyOverSaves(saveName, sourcePath, backupDir);

        RemoveOldRollingBackup(gameName, saveName, saveType);
    }

    private void RemoveOldRollingBackup(string gameName, string saveName, SaveType saveType)
    {
        if (GetCount(gameName, saveName, saveType) > MaxNumberOfRollingBackups)
        {
            DeleteOldest(gameName, saveName, saveType);
        }
    }

    private int GetCount(string gameName, string saveName, SaveType saveType)
    {
        var savePath = backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        if (!Directory.Exists(savePath))
        {
            return 0;
        }

        return Directory.GetDirectories(savePath).Length;
    }

    private void DeleteOldest(string gameName, string saveName, SaveType saveType)
    {
        var oldest = GetOldestPath(gameName, saveName, saveType);
        Directory.Delete(oldest, true);
        logger.LogInformation("Old save {oldest} deleted", oldest);
    }

    private string GetOldestPath(string gameName, string saveName, SaveType saveType)
    {
        var savePath = backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderBy(x => x).First();
    }
}