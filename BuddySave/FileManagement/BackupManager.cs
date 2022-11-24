using BuddySave.Core.Models;
using NLog;

namespace BuddySave.FileManagement;

public class BackupManager : IBackupManager
{
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;
    private readonly ISaveCopier _saveCopier;
    private readonly ILogger _logger;

    public BackupManager(IBackupDirectoryProvider backupDirectoryProvider, ISaveCopier saveCopier, ILogger logger)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
        _saveCopier = saveCopier;
        _logger = logger;
    }
    
    public void BackupFiles(string sourcePath, string saveName, SaveType saveType)
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

        _saveCopier.CopyOverSaves(saveName, sourcePath, _backupDirectoryProvider.Get(saveName, saveType));
    }

    public void RestoreBackup(string destinationPath, string saveName, SaveType saveType)
    {
        var backupDirectory = _backupDirectoryProvider.Get(saveName, saveType);
        _saveCopier.ValidateSource(saveName, backupDirectory);
        _saveCopier.CopyOverSaves(saveName, backupDirectory, destinationPath);
    }
}