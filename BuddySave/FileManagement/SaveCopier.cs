namespace BuddySave.FileManagement;

public class SaveCopier : ISaveCopier
{
    public void ValidateSource(string saveName, string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
        {
            throw new Exception("Source not found! Cannot copy to destination.");
        }

        var files = Directory.GetFiles(sourcePath, GetSaveSearchPattern(saveName));
        if (!files.Any())
        {
            throw new FileNotFoundException($"No files were found in source directory {sourcePath}");
        }
    }

    public void CopyOverSaves(string saveName, string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);
        DeleteOldSave(saveName, destinationPath);

        foreach (var file in Directory.GetFiles(sourcePath, GetSaveSearchPattern(saveName)))
        {
            var fileName = Path.GetFileName(file);
            var destinationFile = Path.Combine(destinationPath, fileName);

            File.Copy(file, destinationFile);
        }
    }

    private static void DeleteOldSave(string saveName, string destinationPath)
    {
        foreach (var oldFile in Directory.GetFiles(destinationPath, GetSaveSearchPattern(saveName)))
        {
            File.Delete(oldFile);
        }
    }

    private static string GetSaveSearchPattern(string saveName)
    {
        return $"{saveName}.*";
    }
}