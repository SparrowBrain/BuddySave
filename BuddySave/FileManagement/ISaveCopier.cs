namespace BuddySave.FileManagement;

public interface ISaveCopier
{
    void ValidateSource(string saveName, string sourcePath);

    void CopyOverSaves(string saveName, string sourcePath, string destinationPath);
}