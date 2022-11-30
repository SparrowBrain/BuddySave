using BuddySave.Core.Models;
using NLog;

namespace BuddySave.FileManagement;

public class RollingBackups : IRollingBackups
{
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;
    private readonly ILogger _logger;

    public RollingBackups(IBackupDirectoryProvider backupDirectoryProvider, ILogger logger)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
        _logger = logger;
    }

    public int GetCount(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        if (!Directory.Exists(savePath))
        {
            return 0;
        }

        return Directory.GetDirectories(savePath).Length;
    }

    public string GetMostRecent(string gameName, string saveName, SaveType saveType)
    {
        var savePath = _backupDirectoryProvider.GetRootDirectory(gameName, saveName, saveType);
        return Directory.GetDirectories(savePath).OrderByDescending(x => x).First();
    }

    public void DeleteOldest(string gameName, string saveName, SaveType saveType)
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