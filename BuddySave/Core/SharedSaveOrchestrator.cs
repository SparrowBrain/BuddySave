using BuddySave.Core.Models;
using BuddySave.Notifications;
using Microsoft.Extensions.Logging;

namespace BuddySave.Core;

public class SharedSaveOrchestrator(
    ILogger<SharedSaveOrchestrator> logger,
    IGameSaveSyncManager gameSaveSyncManager,
    ILockManager lockManager,
    IClientNotifier clientNotifier)
    : ISharedSaveOrchestrator
{
    public async Task<OrchestratorResult> Load(GameSave gameSave, Session session)
    {
        OrchestratorResult result;
        if (lockManager.LockExists(gameSave))
        {
            clientNotifier.Notify("Game save is locked, your friends are playing!");
            return OrchestratorResult.SaveLocked;
        }

        try
        {
            await lockManager.CreateLock(gameSave, session);
            gameSaveSyncManager.DownloadSave(gameSave);
            result = OrchestratorResult.Loaded;
        }
        catch (Exception ex)
        {
            result = OrchestratorResult.Failed;
            logger.LogError(ex, "Error while loading.");
            clientNotifier.Notify("Failed loading game save. Deleting game save lock...");
            await lockManager.DeleteLock(gameSave, session);
            clientNotifier.Notify("Game save lock released.");
        }

        clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
        return result;
    }

    public async Task Save(GameSave gameSave, Session session)
    {
        if (!await lockManager.LockExists(gameSave, session))
        {
            clientNotifier.Notify($"You don't have a lock on a {gameSave.GameName}, cannot save.");
            return;
        }

        try
        {
            clientNotifier.Notify("Uploading game save to cloud...");
            gameSaveSyncManager.UploadSave(gameSave);
            clientNotifier.Notify("Game save uploaded.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while saving.");
            clientNotifier.Notify("Upload failed.");
        }
        finally
        {
            await lockManager.DeleteLock(gameSave, session);
            clientNotifier.Notify("Game save lock released.");
        }
    }
}