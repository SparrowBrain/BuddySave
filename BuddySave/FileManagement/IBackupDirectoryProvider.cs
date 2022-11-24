using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IBackupDirectoryProvider
{
    string Get(string saveName, SaveType saveType);
}