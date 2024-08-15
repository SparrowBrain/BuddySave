using System;
using System.IO;
using System.Threading.Tasks;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement;

public class SaveCopierTests
{
    [Theory, AutoMoqData]
    public async Task CopyOverSaves_DeletesOldDestinationSave_WhenDestinationSaveExists(
        string name,
        string sourceDirName,
        string destinationDirName,
        string oldFile,
        string newFile,
        SaveCopier sut)
    {
        // Arrange
        using var destinationSave = PrepareSaveDirectory(destinationDirName, true);
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);

        var oldSavePath = await PrepareFile($"{name}.{oldFile}", destinationSave);
        await PrepareFile($"{name}.{oldFile}", destinationSave);
        await PrepareFile($"{name}.{newFile}", sourceSave);

        // Act
        sut.CopyOverSaves(name, sourceSave.Path, destinationSave.Path);

        // Assert
        Assert.False(File.Exists(oldSavePath));
    }

    [Theory, AutoMoqData]
    public async Task CopyOverSaves_CreatesDestinationDirectory_WhenDestinationSaveDoesNotExist(
        string name,
        string sourceDirName,
        string destinationDirName,
        string file,
        SaveCopier sut)
    {
        // Arrange
        using var destinationSave = PrepareSaveDirectory(destinationDirName, false);
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);
        await PrepareFile($"{name}.{file}", sourceSave);

        // Act
        sut.CopyOverSaves(name, sourceSave.Path, destinationSave.Path);

        // Assert
        Assert.True(Directory.Exists(destinationSave.Path));
    }

    [Theory, AutoMoqData]
    public async Task CopyOverSaves_CopiesFilesToDestination_WhenSourceHasFiles(
        string name,
        string sourceDirName,
        string destinationDirName,
        string file,
        SaveCopier sut)
    {
        // Arrange
        using var destinationSave = PrepareSaveDirectory(destinationDirName, false);
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);
        await PrepareFile($"{name}.{file}", sourceSave);
        var destinationFilePath = Path.Combine(destinationSave.Path, $"{name}.{file}");

        // Act
        sut.CopyOverSaves(name, sourceSave.Path, destinationSave.Path);

        // Assert
        Assert.True(File.Exists(destinationFilePath));
    }

    [Theory, AutoMoqData]
    public async Task CopyOverSaves_CopiesOnlyOurNamedFiles_WhenSourceHasManyFiles(
        string name,
        string sourceDirName,
        string destinationDirName,
        string[] ourFiles,
        string[] otherFiles,
        SaveCopier sut)
    {
        // Arrange
        using var destinationSave = PrepareSaveDirectory(destinationDirName, true);
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);
        foreach (var file in ourFiles)
        {
            await PrepareFile($"{name}.{file}", sourceSave);
        }

        foreach (var file in otherFiles)
        {
            await PrepareFile(file, sourceSave);
        }

        // Act
        sut.CopyOverSaves(name, sourceSave.Path, destinationSave.Path);

        // Assert
        Assert.Equal(ourFiles.Length, Directory.GetFiles(destinationSave.Path).Length);
    }

    [Theory, AutoMoqData]
    public void ValidateSource_SourceDoesNotExist_Throws(
        string name,
        string sourceDirName,
        SaveCopier sut)
    {
        // Act
        var act = new Action(() => sut.ValidateSource(name, sourceDirName));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Theory, AutoMoqData]
    public void ValidateSource_NoSaveFilesExist_Throws(
        string name,
        string sourceDirName,
        SaveCopier sut)
    {
        // Arrange
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);

        // Act
        var act = new Action(() => sut.ValidateSource(name, sourceSave.Path));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Theory, AutoMoqData]
    public async Task ValidateSource_NoSyncedSaveFilesExistInSource_Throws(
        string name,
        string sourceDirName,
        string file,
        SaveCopier sut)
    {
        // Arrange
        using var sourceSave = PrepareSaveDirectory(sourceDirName, true);
        await PrepareFile(file, sourceSave);

        // Act
        var act = new Action(() => sut.ValidateSource(name, sourceSave.Path));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    private static TempDir PrepareSaveDirectory(string dirName, bool createDirectory)
    {
        return new TempDir(dirName, createDirectory);
    }

    private static async Task<string> PrepareFile(string file, TempDir save)
    {
        var filePath = Path.Combine(save.Path, file);
        await File.WriteAllTextAsync(filePath, "Test");
        return filePath;
    }
}