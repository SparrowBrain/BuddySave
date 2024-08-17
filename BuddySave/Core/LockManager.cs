using System.Text.Json;
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

        var sessionLock = await GetLockedSession(lockPath);
        return string.Equals(sessionLock.UserName, session.UserName);
    }

    public async Task<Session> GetLockedSession(GameSave gameSave)
    {
        var lockPath = GetLockPath(gameSave);
        return await GetLockedSession(lockPath);
    }

    public async Task CreateLock(GameSave gameSave, Session session)
    {
        var lockPath = GetLockPath(gameSave);
        var json = JsonSerializer.Serialize(session);
        await using var fileWriter = new FileStream(lockPath, FileMode.CreateNew);
        await using var streamWriter = new StreamWriter(fileWriter);
        await streamWriter.WriteAsync(json);
    }

    public async Task DeleteLock(GameSave gameSave, Session session)
    {
        var lockPath = GetLockPath(gameSave);
        if (!File.Exists(lockPath))
        {
            return;
        }

        var sessionLock = await GetLockedSession(lockPath);
        if (!string.Equals(session.UserName, sessionLock.UserName))
        {
            throw new Exception($"Cannot delete lock. Lock is owned by {sessionLock.UserName}.");
        }

        File.Delete(lockPath);
    }

    private static string GetLockPath(GameSave gameSave)
    {
        return gameSave.CloudPath + ".lock";
    }

    private static async Task<Session> GetLockedSession(string lockPath)
    {
        var jsonString = await File.ReadAllTextAsync(lockPath);
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            throw new Exception("Cannot get session details from lock. Lock file is empty");
        }

        return JsonSerializer.Deserialize<Session>(jsonString)!;
    }
}