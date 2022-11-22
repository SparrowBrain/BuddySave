namespace BuddySave;

public interface IBackupDirectoryProvider
{
    string Get(string saveName, SaveType saveType);
}