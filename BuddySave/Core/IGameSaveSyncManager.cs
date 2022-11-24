using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IGameSaveSyncManager
{
    void UploadSave(GameSave gameSave);

    void DownloadSave(GameSave gameSave);
}