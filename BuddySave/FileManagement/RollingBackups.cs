using BuddySave.Core.Models;
using NLog;

namespace BuddySave.FileManagement;

public class RollingBackups : IRollingBackups
{
    private const int MaxNumberOfRollingBackups = 10;
    private readonly ILogger _logger;
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;
    private readonly ISaveCopier _saveCopier;

    public RollingBackups(ILogger logger, IBackupDirectoryProvider backupDirectoryProvider, ISaveCopier saveCopier)
    {
        _logger = logger;
        _backupDirectoryProvider = backupDirectoryProvider;
        _saveCopier = saveCopier;
    }

    public string GetMostRecent(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderByDescending(x => x).First();
    }

    public void Add(string sourcePath, string gameName, string saveName, SaveType saveType)
    {
        var backupDir = _backupDirectoryProvider.GetTimestampedDirectory(gameName, saveName, saveType);
        _saveCopier.CopyOverSaves(saveName, sourcePath, backupDir);

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
        var savePath = _backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
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
        _logger.Info($"Old save {oldest} deleted");
    }

    private string GetOldestPath(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderBy(x => x).First();
    }
}