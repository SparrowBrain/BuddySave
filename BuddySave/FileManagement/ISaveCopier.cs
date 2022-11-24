namespace BuddySave.FileManagement;

public interface ISaveCopier
{
    void ValidateSource(string name, string sourcePath);

    void CopyOverSaves(string name, string sourcePath, string destinationPath);
}