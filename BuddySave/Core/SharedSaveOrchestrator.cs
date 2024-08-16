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
    public async Task Load(GameSave gameSave, Session session)
    {
        if (lockManager.LockExists(gameSave))
        {
            clientNotifier.Notify("Game save is locked, your friends are playing!");
            return;
        }

        try
        {
            await lockManager.CreateLock(gameSave, session);
            gameSaveSyncManager.DownloadSave(gameSave);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while loading.");
            clientNotifier.Notify("Failed loading game save. Deleting game save lock...");
            await lockManager.DeleteLock(gameSave, session);
            clientNotifier.Notify("Game save lock released.");
            return;
        }

        clientNotifier.Notify("Game save is prepared! Enjoy Buddy :)");
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