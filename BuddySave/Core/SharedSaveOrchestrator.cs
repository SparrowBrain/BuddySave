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
            var sessionLock = await lockManager.GetSessionLock(gameSave);
            clientNotifier.Notify("Game save is locked, your friends are playing!");
            clientNotifier.Notify($"Connect to {sessionLock.UserName}'s server using {sessionLock.Ip}:{sessionLock.Port}");
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
            gameSaveSyncManager.UploadSave(gameSave);
        }
        catch (Exception ex)
        {
            const string errorLogMessage = "Error while saving.";
            logger.LogError(ex, errorLogMessage);
            clientNotifier.Notify(errorLogMessage);
        }
        finally
        {
            await lockManager.DeleteLock(gameSave, session);
            clientNotifier.Notify("Game save lock released.");
        }
    }
}