using BuddySave.Core.Models;
using BuddySave.Notifications;

namespace BuddySave.Core;

public class SharedSaveOrchestrator : ISharedSaveOrchestrator
{
    private readonly ICloudManager _cloudManager;
    private readonly IClientNotifier _clientNotifier;

    public SharedSaveOrchestrator(ICloudManager cloudManager, IClientNotifier clientNotifier)
    {
        _cloudManager = cloudManager;
        _clientNotifier = clientNotifier;
    }
    
    public async Task Load(GameSave gameSave)
    {
        if (_cloudManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("Game save is locked, your friends are playing!");
            return;
        }

        try
        {
            await _cloudManager.CreateLock(gameSave);
            _cloudManager.DownloadSave(gameSave);
        }
        catch(Exception)
        {
            _clientNotifier.Notify("Failed loading game save. Deleting game save lock...");
            _cloudManager.DeleteLock(gameSave);
            _clientNotifier.Notify("Game save lock released.");
            return;
        }
        
        _clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
    }

    public void Save(GameSave gameSave)
    {
        if (!_cloudManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("There's no lock. Cannot save.");
            return;
        }

        try
        {
            _clientNotifier.Notify("Uploading game save to cloud...");
            _cloudManager.UploadSave(gameSave);
            _clientNotifier.Notify("Game save uploaded.");
        }
        catch (Exception)
        {
            _clientNotifier.Notify("Upload failed.");
        }
        finally
        {
            _cloudManager.DeleteLock(gameSave);
            _clientNotifier.Notify("Game save lock released.");
        }
    }
}