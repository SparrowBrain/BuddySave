using BuddySave.Core.Models;
using BuddySave.System;

namespace BuddySave.FileManagement;

public class LatestSaveTypeProvider(IFileInfoProvider fileInfoProvider) : ILatestSaveTypeProvider
{
    public SaveType Get(GameSave gameSave)
    {
        var latestLocalSaveInfo = fileInfoProvider.Get(gameSave.LocalPath, gameSave.SaveName);
        var latestCloudSaveInfo = fileInfoProvider.Get(gameSave.CloudPath, gameSave.SaveName);
        return latestCloudSaveInfo.LastWriteTimeUtc >= latestLocalSaveInfo.LastWriteTimeUtc ? SaveType.Cloud : SaveType.Local;
    }
}