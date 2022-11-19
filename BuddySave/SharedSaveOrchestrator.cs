namespace BuddySave;

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

        await _cloudManager.CreateLock(gameSave);
        _cloudManager.DownloadSave(gameSave);
        
        _clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
    }

    public void Save(GameSave gameSave)
    {
        if (!_cloudManager.LockExists(gameSave))
        {
            _clientNotifier.Notify("There's no lock. Cannot save.");
            return;
        }
        
        _clientNotifier.Notify("Uploading game save to cloud...");
        _cloudManager.UploadSave(gameSave);
        _cloudManager.DeleteLock(gameSave);
        _clientNotifier.Notify("Game save lock released.");
    }
}