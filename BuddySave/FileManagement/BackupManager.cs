using BuddySave.Core.Models;
using NLog;

namespace BuddySave.FileManagement;

public class BackupManager : IBackupManager
{
    private readonly IRollingBackups _rollingBackups;
    private readonly ISaveCopier _saveCopier;
    private readonly ILogger _logger;

    public BackupManager(IRollingBackups rollingBackups, ISaveCopier saveCopier, ILogger logger)
    {
        _rollingBackups = rollingBackups;
        _saveCopier = saveCopier;
        _logger = logger;
    }

    public void BackupFiles(string sourcePath, string gameName, string saveName, SaveType saveType)
    {
        try
        {
            _saveCopier.ValidateSource(saveName, sourcePath);
        }
        catch
        {
            _logger.Info($"Nothing to backup in {sourcePath}");
            return;
        }

        _rollingBackups.Add(sourcePath, gameName, saveName, saveType);
    }

    public void RestoreBackup(string destinationPath, string gameName, string saveName, SaveType saveType)
    {
        var backupDirectory = _rollingBackups.GetMostRecent(gameName, saveName, saveType);
        _saveCopier.ValidateSource(saveName, backupDirectory);
        _saveCopier.CopyOverSaves(saveName, backupDirectory, destinationPath);
    }
}