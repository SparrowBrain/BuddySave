using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.System;
using BuddySave.TestTools;
using Moq;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement;

public class LatestSaveTypeProviderTests
{
    [Theory, AutoMoqData]
    public async Task ReturnsLocalSaveType_When_LocalSaveIsNewer(
        [Frozen] Mock<IFileInfoProvider> fileInfoProviderMock,
        GameSave gameSave,
        LatestSaveTypeProvider sut)
    {
        // Arrange
        var localFileInfo = await PrepareFile(SaveType.Local.ToString(), gameSave.SaveName, DateTime.Now);
        var cloudFileInfo = await PrepareFile(SaveType.Cloud.ToString(), gameSave.SaveName, DateTime.Now.AddDays(-1));
        
        fileInfoProviderMock.Setup(x => x.Get(gameSave.LocalPath, gameSave.SaveName)).Returns(localFileInfo);
        fileInfoProviderMock.Setup(x => x.Get(gameSave.CloudPath, gameSave.SaveName)).Returns(cloudFileInfo);
        
        // Act
        var result = sut.Get(gameSave);
    
        // Assert
        Assert.Equal(SaveType.Local, result);
    }
    
    [Theory, AutoMoqData]
    public async Task ReturnsCloudSaveType_When_CloudSaveIsNewer(
        [Frozen] Mock<IFileInfoProvider> fileInfoProviderMock,
        GameSave gameSave,
        LatestSaveTypeProvider sut)
    {
        // Arrange
        var localFileInfo = await PrepareFile(SaveType.Local.ToString(), gameSave.SaveName, DateTime.Now.AddDays(-1));
        var cloudFileInfo = await PrepareFile(SaveType.Cloud.ToString(), gameSave.SaveName, DateTime.Now);
        
        fileInfoProviderMock.Setup(x => x.Get(gameSave.LocalPath, gameSave.SaveName)).Returns(localFileInfo);
        fileInfoProviderMock.Setup(x => x.Get(gameSave.CloudPath, gameSave.SaveName)).Returns(cloudFileInfo);
        
        // Act
        var result = sut.Get(gameSave);
    
        // Assert
        Assert.Equal(SaveType.Cloud, result);
    }
    
    [Theory, AutoMoqData]
    public async Task ReturnsCloudSaveType_When_LocalSaveTimeIsEqual(
        [Frozen] Mock<IFileInfoProvider> fileInfoProviderMock,
        GameSave gameSave,
        LatestSaveTypeProvider sut)
    {
        // Arrange
        var localFileInfo = await PrepareFile(SaveType.Local.ToString(), gameSave.SaveName, DateTime.Now);
        var cloudFileInfo = await PrepareFile(SaveType.Cloud.ToString(), gameSave.SaveName, DateTime.Now);
        
        fileInfoProviderMock.Setup(x => x.Get(gameSave.LocalPath, gameSave.SaveName)).Returns(localFileInfo);
        fileInfoProviderMock.Setup(x => x.Get(gameSave.CloudPath, gameSave.SaveName)).Returns(cloudFileInfo);
        
        // Act
        var result = sut.Get(gameSave);
    
        // Assert
        Assert.Equal(SaveType.Cloud, result);
    }

    private static async Task<FileInfo> PrepareFile(string directoryName, string file, DateTime lastWriteTimeUtc)
    {
        var saveDirectory = new TempDir(directoryName, true);
        var filePath = Path.Combine(saveDirectory.Path, file);
        await File.WriteAllTextAsync(filePath, "Test");
        return new FileInfo(filePath)
        {
            LastWriteTimeUtc = lastWriteTimeUtc
        };
    }
}