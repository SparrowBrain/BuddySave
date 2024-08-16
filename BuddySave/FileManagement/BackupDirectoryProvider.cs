using BuddySave.Core.Models;
using BuddySave.System;

namespace BuddySave.FileManagement;

public class BackupDirectoryProvider(IDateTimeProvider dateTimeProvider) : IBackupDirectoryProvider
{
    private const string BackupDirectoryPrefix = "SavesBackup";

    public string GetTimestampedDirectory(string gameName, string saveName, SaveType saveType)
    {
        return Path.Combine(GetRootDirectory(gameName, saveName, saveType), dateTimeProvider.Now().ToString("yyyyMMdd_HHmmss"));
    }

    public string GetRootDirectory(string gameName, string saveName, SaveType saveType)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BackupDirectoryPrefix, gameName, saveName, saveType.ToString());
    }
}