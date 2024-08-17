namespace BuddySave.System;

public interface IFileInfoProvider
{
    FileInfo Get(string path, string saveName);
}