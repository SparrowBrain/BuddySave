using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IBackupDirectoryProvider
{
    string GetNew(string gameName, string saveName, SaveType saveType);

    string GetRootGameSavePath(string gameName, string saveName, SaveType saveType);
}
