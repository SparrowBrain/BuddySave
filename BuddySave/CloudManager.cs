namespace BuddySave;

public class CloudManager : ICloudManager
{
    public void UploadSave(GameSave gameSave)
    {
        throw new NotImplementedException();
    }

    public void DownloadSave(GameSave gameSave)
    {
        throw new NotImplementedException();
    }

    public bool LockExists(GameSave save)
    {
        throw new NotImplementedException();
    }

    public Task CreateLock(GameSave save)
    {
        throw new NotImplementedException();
    }

    public void DeleteLock(GameSave save)
    {
        throw new NotImplementedException();
    }
}