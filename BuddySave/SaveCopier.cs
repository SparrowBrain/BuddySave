namespace BuddySave;

public class SaveCopier : ISaveCopier
{
    public void ValidateSource(string name, string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
        {
            throw new Exception("Source not found! Cannot copy to destination.");
        }

        var files = Directory.GetFiles(sourcePath, GetSaveSearchPattern(name));
        if (!files.Any())
        {
            throw new FileNotFoundException($"No files were found in source directory {sourcePath}");
        }
    }

    public void CopyOverSaves(string name, string sourcePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);
        DeleteOldSave(name, destinationPath);

        foreach (var file in Directory.GetFiles(sourcePath, GetSaveSearchPattern(name)))
        {
            var fileName = Path.GetFileName(file);
            var destinationFile = Path.Combine(destinationPath, fileName);

            File.Copy(file, destinationFile);

            // TODO: check if file is a folder and copy files with recursion if it is
        }
    }

    private static void DeleteOldSave(string name, string destinationPath)
    {
        foreach (var oldFile in Directory.GetFiles(destinationPath, GetSaveSearchPattern(name)))
        {
            File.Delete(oldFile);
        }
    }

    private static string GetSaveSearchPattern(string name)
    {
        return $"{name}.*";
    }
}