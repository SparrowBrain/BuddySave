using System;
using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Moq;
using Xunit;

namespace BuddySave.UnitTests.FileManagement;

public class BackupManagerTests
{
    [Theory, AutoMoqData]
    public void BackupFile_DoesNothing_When_SaveDoesNotExist(
        string saveName,
        string savePath,
        SaveType saveType,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        BackupManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        sut.BackupFiles(savePath, saveName, saveType);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [Theory, AutoMoqData]
    public void BackupFile_DoesNotThrowException_When_SaveDoesNotExist(
        string saveName,
        string savePath,
        SaveType saveType,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        BackupManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(saveName, savePath)).Throws(new Exception());

        // Act
        var result = Record.Exception(() => sut.BackupFiles(savePath, saveName, saveType));
        
        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineAutoMoqData(SaveType.Cloud)]
    [InlineAutoMoqData(SaveType.Local)]
    public void BackupFile_BackupSaves_When_SavesExist(
        SaveType saveType,
        string saveName,
        string savePath,
        string backupDirectory,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
        BackupManager sut)
    {
        // Arrange
        backupDirectoryProviderMock.Setup(x => x.Get(saveName, saveType)).Returns(backupDirectory);
        
        // Act
        sut.BackupFiles(savePath, saveName, saveType);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(saveName, savePath, backupDirectory), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public void RestoreBackup_Throws_When_SaveDoesNotExist(
        string backupDirectory,
        string saveName,
        SaveType saveType,
        [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        BackupManager sut)
    {
        // Arrange
        backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<SaveType>())).Returns(backupDirectory);
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        var act = new Action(() => sut.RestoreBackup(backupDirectory, saveName, saveType));
        
        // Assert
        Assert.ThrowsAny<Exception>(act);
    }
    
    [Theory]
    [InlineAutoMoqData(SaveType.Cloud)]
    [InlineAutoMoqData(SaveType.Local)]
    public void RestoreBackup_RestoresSaves_When_BackupSavesExist(
        SaveType saveType,
        string saveName,
        string savePath,
        string backupDirectory,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
        BackupManager sut)
    {
        // Arrange
        backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<SaveType>())).Returns(backupDirectory);
        
        // Act
        sut.RestoreBackup(savePath, saveName, saveType);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(saveName, backupDirectory, savePath));
    }
}