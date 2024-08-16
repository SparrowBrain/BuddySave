using BuddySave.Core.Models;
using BuddySave.FileManagement;
using Microsoft.Extensions.Logging;

namespace BuddySave.Core;

public class GameSaveSyncManager(
    ILogger<GameSaveSyncManager> logger, 
    ISaveCopier saveCopier, 
    IBackupManager backupManager)
    : IGameSaveSyncManager
{
    public void UploadSave(GameSave gameSave)
    {
        saveCopier.ValidateSource(gameSave.SaveName, gameSave.LocalPath);
        backupManager.BackupFiles(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);

        try
        {
            saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.LocalPath, gameSave.CloudPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Upload failed.");
            backupManager.RestoreBackup(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);
        }
    }

    public void DownloadSave(GameSave gameSave)
    {
        saveCopier.ValidateSource(gameSave.SaveName, gameSave.CloudPath);
        backupManager.BackupFiles(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);

        try
        {
            saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.CloudPath, gameSave.LocalPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Download failed.");
            backupManager.RestoreBackup(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);
        }
    }
}