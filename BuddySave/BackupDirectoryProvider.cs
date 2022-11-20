﻿namespace BuddySave;

public class BackupDirectoryProvider : IBackupDirectoryProvider
{
    private const string BackupDirectoryPrefix = "SavesBackup";

    public string Get(string saveName, SaveType saveType)
    {
        return BackupDirectoryPrefix + saveName + saveType;
    }
}