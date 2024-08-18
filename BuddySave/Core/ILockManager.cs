using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface ILockManager
{
    bool LockExists(GameSave gameSave);
    
    Task<bool> LockExists(GameSave gameSave, Session session);
    
    Task<Session> GetLockedSession(GameSave gameSave);

    Task CreateLock(GameSave gameSave, Session session);

    Task DeleteLock(GameSave gameSave, Session session);
}