namespace BuddySave;

public class CloudManager : ICloudManager
{
    private readonly IBackupDirectoryProvider _backupDirectoryProvider;

    public CloudManager(IBackupDirectoryProvider backupDirectoryProvider)
    {
        _backupDirectoryProvider = backupDirectoryProvider;
    }
    
    public void UploadSave(GameSave gameSave)
    {
        ValidateSource(gameSave.LocalPath);
        BackupFiles(gameSave.CloudPath, gameSave.Name, SaveType.Cloud);

        try
        {
            CopyOverSaves(gameSave.LocalPath, gameSave.CloudPath);
        }
        catch (Exception)
        {
            RestoreBackup(gameSave.CloudPath, gameSave.Name, SaveType.Cloud);
        }
    }

    public void DownloadSave(GameSave gameSave)
    {
        ValidateSource(gameSave.CloudPath);
        BackupFiles(gameSave.LocalPath, gameSave.Name, SaveType.Local);

        try
        {
            CopyOverSaves(gameSave.CloudPath, gameSave.LocalPath);
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

    private static void ValidateSource(string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
        {
            throw new Exception("Source not found! Cannot copy to destination.");
        }

        var files = Directory.GetFiles(sourcePath);
        if (!files.Any())
        {
            throw new FileNotFoundException($"No files were found in source directory {sourcePath}");
        }
    }

    private void BackupFiles(string sourcePath, string saveName, SaveType saveType)
    {
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        var files = Directory.GetFiles(sourcePath);
        if (!files.Any())
        {
            return;
        }
        
        CopyOverSaves(sourcePath, _backupDirectoryProvider.Get(saveName, saveType));
    }

    private void RestoreBackup(string destinationPath, string saveName, SaveType saveType)
    {
        CopyOverSaves(_backupDirectoryProvider.Get(saveName, saveType), destinationPath);
    }
    
    private static void CopyOverSaves(string sourcePath, string destinationPath)
    {
        if (Directory.Exists(destinationPath))
        {
            Directory.Delete(destinationPath, true);
        }

        Directory.CreateDirectory(destinationPath);
        
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var destinationFile = Path.Combine(destinationPath, fileName);

            File.Copy(file, destinationFile);

            // TODO: check if file is a folder and copy files with recursion if it is
        }
    }

    private static string GetLockPath(GameSave save)
    {
        return save.CloudPath + ".lock";
    }
}