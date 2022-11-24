using BuddySave.Core.Models;
using BuddySave.Notifications;

namespace BuddySave.Core;

public class SharedSaveOrchestrator : ISharedSaveOrchestrator
{
    private readonly IGameSaveSyncManager _gameSaveSyncManager;
    private readonly ILockManager _lockManager;
    private readonly IClientNotifier _clientNotifier;

    public SharedSaveOrchestrator(IGameSaveSyncManager gameSaveSyncManager, ILockManager lockManager, IClientNotifier clientNotifier)
    {
        _gameSaveSyncManager = gameSaveSyncManager;
        _lockManager = lockManager;
        _clientNotifier = clientNotifier;
    }
    
    public async Task Load(GameSave gameSave)
    {
        if (_lockManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("Game save is locked, your friends are playing!");
            return;
        }

        try
        {
            await _lockManager.CreateLock(gameSave);
            _gameSaveSyncManager.DownloadSave(gameSave);
        }
        catch(Exception)
        {
            _clientNotifier.Notify("Failed loading game save. Deleting game save lock...");
            _lockManager.DeleteLock(gameSave);
            _clientNotifier.Notify("Game save lock released.");
            return;
        }
        
        _clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
    }

    public void Save(GameSave gameSave)
    {
        if (!_lockManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("There's no lock. Cannot save.");
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
            _lockManager.DeleteLock(gameSave);
            _clientNotifier.Notify("Game save lock released.");
        }
    }
}