using BuddySave.Core.Models;

namespace BuddySave.Core;

public class LockManager : ILockManager
{
    public bool LockExists(GameSave gameSave)
    {
        var lockPath = GetLockPath(gameSave);
        return File.Exists(lockPath);
    }

    public async Task<bool> LockExists(GameSave gameSave, Session session)
    {
        var lockPath = GetLockPath(gameSave);
        if (!File.Exists(lockPath))
        {
            return false;
        }
        
        var lockedByUserName = await GetUserNameWhoHasLock(lockPath);
        return string.Equals(lockedByUserName, session.UserName);
    }

    public async Task CreateLock(GameSave gameSave, Session session)
    {
        var lockPath = GetLockPath(gameSave);
        await using var fileWriter = new FileStream(lockPath, FileMode.CreateNew);
        await using var streamWriter = new StreamWriter(fileWriter);
        await streamWriter.WriteLineAsync(session.UserName);
    }

    public async Task DeleteLock(GameSave gameSave, Session session)
    {
        var lockPath = GetLockPath(gameSave);

        if (!File.Exists(lockPath))
        {
            return;
        }
        
        var lockedByUserName = await GetUserNameWhoHasLock(lockPath);
        if (!string.Equals(session.UserName, lockedByUserName))
        {
            throw new Exception($"Cannot delete lock. Lock is owned by {lockedByUserName}.");
        }
        
        File.Delete(lockPath);
    }

    private static string GetLockPath(GameSave gameSave)
    {
        return gameSave.CloudPath + ".lock";
    }

    private static async Task<string> GetUserNameWhoHasLock(string path)
    {
        var userName = (await File.ReadAllTextAsync(path)).Trim();
        return userName;
    }
}