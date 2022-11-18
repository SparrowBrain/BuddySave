namespace BuddySave;

internal interface ICloudManager
{
    void UploadSave(GameSave gameSave);

    void DownloadSave(GameSave gameSave);

    bool LockExists(GameSave save);

    void CreateLock(GameSave save);

    void DeleteLock(GameSave save);
}