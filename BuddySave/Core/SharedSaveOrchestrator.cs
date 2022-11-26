using BuddySave.Core.Models;
using BuddySave.Notifications;

namespace BuddySave.Core;

public class SharedSaveOrchestrator : ISharedSaveOrchestrator
{
    private readonly IGameSaveSyncManager _gameSaveSyncManager;
    private readonly ILockManager _lockManager;
    private readonly IClientNotifier _clientNotifier;

    public SharedSaveOrchestrator(
        IGameSaveSyncManager gameSaveSyncManager, 
        ILockManager lockManager, 
        IClientNotifier clientNotifier)
    {
        _gameSaveSyncManager = gameSaveSyncManager;
        _lockManager = lockManager;
        _clientNotifier = clientNotifier;
    }
    
    public async Task Load(GameSave gameSave, Session session)
    {
        if (_lockManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("Game save is locked, your friends are playing!");
            return;
        }

        try
        {
            await _lockManager.CreateLock(gameSave, session);
            _gameSaveSyncManager.DownloadSave(gameSave);
        }
        catch(Exception)
        {
            _clientNotifier.Notify("Failed loading game save. Deleting game save lock...");
            await _lockManager.DeleteLock(gameSave, session);
            _clientNotifier.Notify("Game save lock released.");
            return;
        }
        
        _clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
    }

    public async Task Save(GameSave gameSave, Session session)
    {
        if (!await _lockManager.LockExists(gameSave, session))
        {
            _clientNotifier.Notify($"You don't have a lock on a {gameSave.GameName}, cannot save.");
            return;
        }

        try
        {
            _clientNotifier.Notify("Uploading game save to cloud...");
            _gameSaveSyncManager.UploadSave(gameSave);
            _clientNotifier.Notify("Game save uploaded.");
        }
        catch (Exception)
        {
            _clientNotifier.Notify("Upload failed.");
        }
        finally
        {
            await _lockManager.DeleteLock(gameSave, session);
            _clientNotifier.Notify("Game save lock released.");
        }
    }
}