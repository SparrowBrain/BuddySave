using BuddySave.Core.Models;

namespace BuddySave.System;

public class FileInfoProvider : IFileInfoProvider
{
    public FileInfo Get(string path, string saveName)
    {
        var directoryInfo = new DirectoryInfo(path);
        return directoryInfo.GetFiles($"{saveName}.*").OrderByDescending(x => x.LastWriteTime).First();
    }
}