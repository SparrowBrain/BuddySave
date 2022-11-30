using BuddySave.Core.Models;
using BuddySave.FileManagement;

namespace BuddySave.Core;

public class GameSaveSyncManager : IGameSaveSyncManager
{
    private readonly ISaveCopier _saveCopier;
    private readonly IBackupManager _backupManager;

    public GameSaveSyncManager(ISaveCopier saveCopier, IBackupManager backupManager)
    {
        _saveCopier = saveCopier;
        _backupManager = backupManager;
    }

    public void UploadSave(GameSave gameSave)
    {
        _saveCopier.ValidateSource(gameSave.SaveName, gameSave.LocalPath);
        _backupManager.BackupFiles(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);

        try
        {
            _saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.LocalPath, gameSave.CloudPath);
        }
        catch (Exception)
        {
            _backupManager.RestoreBackup(gameSave.CloudPath, gameSave.GameName, gameSave.SaveName, SaveType.Cloud);
        }
    }

    public void DownloadSave(GameSave gameSave)
    {
        _saveCopier.ValidateSource(gameSave.SaveName, gameSave.CloudPath);
        _backupManager.BackupFiles(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);

        try
        {
            _saveCopier.CopyOverSaves(gameSave.SaveName, gameSave.CloudPath, gameSave.LocalPath);
        }
        catch (Exception)
        {
            _backupManager.RestoreBackup(gameSave.LocalPath, gameSave.GameName, gameSave.SaveName, SaveType.Local);
        }
    }
}