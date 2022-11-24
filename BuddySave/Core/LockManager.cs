using BuddySave.Core.Models;

namespace BuddySave.Core;

public class LockManager : ILockManager
{
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

    private static string GetLockPath(GameSave save)
    {
        return save.CloudPath + ".lock";
    }
}