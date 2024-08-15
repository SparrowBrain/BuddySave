using BuddySave.Core.Models;
using Microsoft.Extensions.Logging;

namespace BuddySave.FileManagement;

public class BackupManager(
    IRollingBackups rollingBackups, 
    ISaveCopier saveCopier, 
    ILogger<BackupManager> logger)
    : IBackupManager
{
    public void BackupFiles(string sourcePath, string gameName, string saveName, SaveType saveType)
    {
        try
        {
            saveCopier.ValidateSource(saveName, sourcePath);
        }
        catch
        {
            logger.LogInformation("Nothing to backup in {sourcePath}", sourcePath);
            return;
        }

        rollingBackups.Add(sourcePath, gameName, saveName, saveType);
    }

    public void RestoreBackup(string destinationPath, string gameName, string saveName, SaveType saveType)
    {
        var backupDirectory = rollingBackups.GetMostRecent(gameName, saveName, saveType);
        saveCopier.ValidateSource(saveName, backupDirectory);
        saveCopier.CopyOverSaves(saveName, backupDirectory, destinationPath);
    }
}