using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface ILockManager
{
    bool LockExists(GameSave save);

    Task CreateLock(GameSave save);

    void DeleteLock(GameSave save);
}