using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface IBackupManager
{
    void BackupFiles(string sourcePath, string gameName, string saveName, SaveType saveType);

    void RestoreBackup(string destinationPath, string gameName, string saveName, SaveType saveType);
}