using BuddySave.Core.Models;
using BuddySave.System;

namespace BuddySave.FileManagement;

public class BackupDirectoryProvider : IBackupDirectoryProvider
{
    private const string BackupDirectoryPrefix = "SavesBackup";
    private readonly IDateTimeProvider _dateTimeProvider;

    public BackupDirectoryProvider(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public string GetTimestampedDirectory(string gameName, string saveName, SaveType saveType)
    {
        return Path.Combine(GetRootDirectory(gameName, saveName, saveType), _dateTimeProvider.Now().ToString("yyyyMMdd_HHmmss"));
    }

    public string GetRootDirectory(string gameName, string saveName, SaveType saveType)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BackupDirectoryPrefix, gameName, saveName, saveType.ToString());
    }
}