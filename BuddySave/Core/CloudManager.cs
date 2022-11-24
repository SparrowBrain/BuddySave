using BuddySave.Core.Models;
using BuddySave.FileManagement;
using NLog;

namespace BuddySave.Core;

public class CloudManager : ICloudManager
{
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;
    private readonly ISaveCopier _saveCopier;
    private readonly ILogger _logger;

    public CloudManager(IBackupDirectoryProvider backupDirectoryProvider, ISaveCopier saveCopier, ILogger logger)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
        _saveCopier = saveCopier;
        _logger = logger;
    }

    public void UploadSave(GameSave gameSave)
    {
        _saveCopier.ValidateSource(gameSave.Name, gameSave.LocalPath);
        BackupFiles(gameSave.CloudPath, gameSave.Name, SaveType.Cloud);

        try
        {
            _saveCopier.CopyOverSaves(gameSave.Name, gameSave.LocalPath, gameSave.CloudPath);
        }
        catch (Exception)
        {
            RestoreBackup(gameSave.CloudPath, gameSave.Name, SaveType.Cloud);
        }
    }

    public void DownloadSave(GameSave gameSave)
    {
        _saveCopier.ValidateSource(gameSave.Name, gameSave.CloudPath);
        BackupFiles(gameSave.LocalPath, gameSave.Name, SaveType.Local);

        try
        {
            _saveCopier.CopyOverSaves(gameSave.Name, gameSave.CloudPath, gameSave.LocalPath);
        }
        catch (Exception)
        {
            RestoreBackup(gameSave.LocalPath, gameSave.Name, SaveType.Local);
        }
    }

    public bool LockExists(GameSave save)
    {
        var lockPath = GetLockPath(save);
        return File.Exists(lockPath);
    }

    public async Task CreateLock(GameSave save)
    {
        var lockPath = GetLockPath(save);
        await using var fileWriter = new FileStream(lockPath, FileMode.CreateNew);
        await using var streamWriter = new StreamWriter(fileWriter);
        await streamWriter.WriteLineAsync(DateTime.Now.ToString("o"));
    }

    public void DeleteLock(GameSave save)
    {
        var lockPath = GetLockPath(save);
        File.Delete(lockPath);
    }

    private void BackupFiles(string sourcePath, string saveName, SaveType saveType)
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

    private void RestoreBackup(string destinationPath, string saveName, SaveType saveType)
    {
        _saveCopier.CopyOverSaves(saveName, _backupDirectoryProvider.Get(saveName, saveType), destinationPath);
    }

    private static string GetLockPath(GameSave save)
    {
        return save.CloudPath + ".lock";
    }
}