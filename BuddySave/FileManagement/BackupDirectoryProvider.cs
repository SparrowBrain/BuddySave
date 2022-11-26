using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public class BackupDirectoryProvider : IBackupDirectoryProvider
{
    private const string BackupDirectoryPrefix = "SavesBackup";

    public string Get(string saveName, SaveType saveType)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{BackupDirectoryPrefix}_{saveName}_{SaveType.Cloud}");
    }
}