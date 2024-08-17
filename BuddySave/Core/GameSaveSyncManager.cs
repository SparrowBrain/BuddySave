using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using Microsoft.Extensions.Logging;

namespace BuddySave.Core;

public class GameSaveSyncManager(
    ILogger<GameSaveSyncManager> logger, 
    ISaveCopier saveCopier, 
    IBackupManager backupManager,
    ILatestSaveTypeProvider latestSaveTypeProvider,
    IClientNotifier clientNotifier)
    : IGameSaveSyncManager
{
    public void UploadSave(GameSave gameSave)
    {
        saveCopier.ValidateSource(gameSave.SaveName, gameSave.LocalPath);
        backupManager.BackupFiles(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);

        try
        {
            if (latestSaveTypeProvider.Get(gameSave) is SaveType.Cloud)
            {
                const string skippedLogMessage = "Newer save found in Cloud, uploading game save skipped!!";
                logger.LogInformation(skippedLogMessage);
                clientNotifier.Notify(skippedLogMessage);
                return;
            }
            
            clientNotifier.Notify("Uploading game save to cloud...");
            saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.LocalPath, gameSave.CloudPath);
            clientNotifier.Notify("Game save uploaded.");
        }
        catch (Exception ex)
        {
            const string failedLogMessage = "Upload failed.";
            logger.LogError(ex, failedLogMessage);
            clientNotifier.Notify(failedLogMessage);
            backupManager.RestoreBackup(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);
            throw;
        }
    }

    public void DownloadSave(GameSave gameSave)
    {
        saveCopier.ValidateSource(gameSave.SaveName, gameSave.CloudPath);
        backupManager.BackupFiles(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);

        try
        {
            if (latestSaveTypeProvider.Get(gameSave) is SaveType.Local)
            {
                const string skippedLogMessage = "Newer Local game save was found, downloading game save skipped!!";
                logger.LogInformation(skippedLogMessage);
                clientNotifier.Notify(skippedLogMessage);
                return;
            }

            clientNotifier.Notify("Downloading game save from cloud...");
            saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.CloudPath, gameSave.LocalPath);
            clientNotifier.Notify("Game save downloaded.");
        }
        catch (Exception ex)
        {
            const string failedLogMessage = "Download failed.";
            logger.LogError(ex, failedLogMessage);
            clientNotifier.Notify(failedLogMessage);
            backupManager.RestoreBackup(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);
            throw;
        }
    }
}