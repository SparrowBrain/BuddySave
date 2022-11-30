﻿using BuddySave.Core.Models;
using NLog;

namespace BuddySave.FileManagement;

public class BackupManager : IBackupManager
{
    private const int MaxNumberOfRollingBackups = 10;
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;
    private readonly IRollingBackups _rollingBackups;
    private readonly ISaveCopier _saveCopier;
    private readonly ILogger _logger;

    public BackupManager(IBackupDirectoryProvider backupDirectoryProvider, IRollingBackups rollingBackups, ISaveCopier saveCopier, ILogger logger)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
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

        _saveCopier.CopyOverSaves(saveName, sourcePath, _backupDirectoryProvider.GetNew(gameName, saveName, saveType));
        RemoveOldRollingBackup(gameName, saveName, saveType);
    }

    public void RestoreBackup(string destinationPath, string gameName, string saveName, SaveType saveType)
    {
        var backupDirectory = _rollingBackups.GetMostRecentPath(gameName, saveName, saveType);
        _saveCopier.ValidateSource(saveName, backupDirectory);
        _saveCopier.CopyOverSaves(saveName, backupDirectory, destinationPath);
    }

    private void RemoveOldRollingBackup(string gameName, string saveName, SaveType saveType)
    {
        if (_rollingBackups.GetCount(gameName, saveName, saveType) > MaxNumberOfRollingBackups)
        {
            _rollingBackups.DeleteOldestSave(gameName, saveName, saveType);
        }
    }
}