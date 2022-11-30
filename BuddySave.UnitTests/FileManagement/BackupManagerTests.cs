using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Moq;
using NLog;
using System;
using Xunit;

namespace BuddySave.UnitTests.FileManagement;

public class BackupManagerTests
{
    [Theory, AutoMoqData]
    public void BackupFiles_DoesNothing_When_SaveDoesNotExist(
        string gameName,
        string saveName,
        string savePath,
        SaveType saveType,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILogger> loggerMock,
        BackupManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        sut.BackupFiles(savePath, gameName, saveName, saveType);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(x => x.Info($"Nothing to backup in {savePath}"));
    }

    [Theory, AutoMoqData]
    public void BackupFiles_DoesNotThrowException_When_SaveDoesNotExist(
        string gameName,
        string saveName,
        string savePath,
        SaveType saveType,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        BackupManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(saveName, savePath)).Throws(new Exception());

        // Act
        var result = Record.Exception(() => sut.BackupFiles(savePath, gameName, saveName, saveType));

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineAutoMoqData(SaveType.Cloud)]
    [InlineAutoMoqData(SaveType.Local)]
    public void BackupFiles_BackupSaves_When_SavesExist(
        SaveType saveType,
        string gameName,
        string saveName,
        string savePath,
        [Frozen] Mock<IRollingBackups> rollingBackupsMock,
        BackupManager sut)
    {
        // Act
        sut.BackupFiles(savePath, gameName, saveName, saveType);

        // Assert
        rollingBackupsMock.Verify(x => x.Add(savePath, gameName, saveName, saveType), Times.Once);
    }

    [Theory, AutoMoqData]
    public void RestoreBackup_Throws_When_SaveDoesNotExist(
        string backupDirectory,
        string gameName,
        string saveName,
        SaveType saveType,
        [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        BackupManager sut)
    {
        // Arrange
        backupDirectoryProviderMock.Setup(x => x.GetTimestampedDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>())).Returns(backupDirectory);
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        var act = new Action(() => sut.RestoreBackup(backupDirectory, gameName, saveName, saveType));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Theory]
    [InlineAutoMoqData(SaveType.Cloud)]
    [InlineAutoMoqData(SaveType.Local)]
    public void RestoreBackup_RestoresMostRecentSave_When_BackupSavesExist(
        SaveType saveType,
        string gameName,
        string saveName,
        string savePath,
        string mostRecentBackupDirectory,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<IRollingBackups> rollingBackupsMock,
        BackupManager sut)
    {
        // Arrange
        rollingBackupsMock.Setup(x => x.GetMostRecent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>())).Returns(mostRecentBackupDirectory);

        // Act
        sut.RestoreBackup(savePath, gameName, saveName, saveType);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(saveName, mostRecentBackupDirectory, savePath));
    }
}