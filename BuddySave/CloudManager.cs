namespace BuddySave;

public class CloudManager : ICloudManager
{
    public void UploadSave(GameSave gameSave)
    {
        CopyOverSaves(gameSave.LocalPath, gameSave.CloudPath);
    }

    public void DownloadSave(GameSave gameSave)
    {
        CopyOverSaves(gameSave.CloudPath, gameSave.LocalPath);
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

    private static void CopyOverSaves(string sourcePath, string destinationPath)
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