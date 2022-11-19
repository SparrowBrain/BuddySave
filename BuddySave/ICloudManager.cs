namespace BuddySave;

public interface ICloudManager
{
    void UploadSave(GameSave gameSave);

    void DownloadSave(GameSave gameSave);

    bool LockExists(GameSave save);

    Task CreateLock(GameSave save);

    void DeleteLock(GameSave save);
}