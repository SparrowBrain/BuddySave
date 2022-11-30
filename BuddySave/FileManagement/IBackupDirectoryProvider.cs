using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IBackupDirectoryProvider
{
    string GetTimestampedDirectory(string gameName, string saveName, SaveType saveType);

    string GetRootDirectory(string gameName, string saveName, SaveType saveType);
}
