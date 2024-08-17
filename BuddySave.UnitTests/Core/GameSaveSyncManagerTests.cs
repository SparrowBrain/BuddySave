using System;
using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using BuddySave.TestTools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class GameSaveSyncManagerTests
{
    [Theory, AutoMoqData]
    public void UploadSave_Throws_When_SourceValidationFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        var act = new Action(() => sut.UploadSave(save));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Theory, AutoMoqData]
    public void UploadSave_DoesNotUploadSave_When_CloudSaveIsNewer(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Cloud);

        // Act
        sut.UploadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.LocalPath, save.CloudPath), Times.Never);
    }

    [Theory, AutoMoqData]
    public void UploadSave_LogInformation_When_CloudSaveIsNewer(
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        [Frozen] Mock<ILogger<GameSaveSyncManager>> loggerMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Cloud);

        // Act
        sut.UploadSave(save);

        // Assert
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Newer save found in Cloud, uploading game save skipped!!")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_CopiesOverSaves_When_LocalSaveIsNewer(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        sut.UploadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.LocalPath, save.CloudPath), Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_NotifyClient_When_SaveIsUploaded(
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        sut.UploadSave(save);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Uploading game save to cloud..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save uploaded."), Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_BacksUpCloud_When_CloudHasASave(
        [Frozen] Mock<IBackupManager> backupManagerMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Act
        sut.UploadSave(save);

        // Assert
        backupManagerMock.Verify(x => x.BackupFiles(save.CloudPath, save.GameName, save.SaveName, SaveType.Cloud), Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_CopiesLocalSavesToCloud_When_LocalSaveIsValid(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange 
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        sut.UploadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.LocalPath, save.CloudPath), Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_RestoresCloudBackup_When_UploadFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<IBackupManager> backupManagerMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception());
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        var act = new Action(() => sut.UploadSave(save));

        // Assert
        Assert.Throws<Exception>(act);
        backupManagerMock.Verify(x => x.RestoreBackup(save.CloudPath, save.GameName, save.SaveName, SaveType.Cloud), Times.Once);
    }

    [Theory, AutoMoqData]
    public void UploadSave_LogsError_When_UploadFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILogger<GameSaveSyncManager>> loggerMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        Exception exception,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(exception);
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        var act = new Action(() => sut.UploadSave(save));

        // Assert
        Assert.Throws<Exception>(act);
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Upload failed.")),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_Throws_When_SourceValidationFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

        // Act
        var act = new Action(() => sut.DownloadSave(save));

        // Assert
        Assert.ThrowsAny<Exception>(act);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_BacksUpLocal_When_LocalHasASave(
        [Frozen] Mock<IBackupManager> backupManagerMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Act
        sut.DownloadSave(save);

        // Assert
        backupManagerMock.Verify(x => x.BackupFiles(save.LocalPath, save.GameName, save.SaveName, SaveType.Local), Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_CopiesCloudSavesToLocal_When_CloudSaveIsValid(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Act
        sut.DownloadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath), Times.Once());
    }

    [Theory, AutoMoqData]
    public void DownloadSave_NotifiesClient_When_CloudSaveIsValid(
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Act
        sut.DownloadSave(save);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Downloading game save from cloud..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save downloaded."), Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_DoNotDownloadSave_When_LocalSaveIsNewer(
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        sut.DownloadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath), Times.Never);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_LogsInformation_When_LocalSaveIsNewer(
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        [Frozen] Mock<ILogger<GameSaveSyncManager>> loggerMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Local);

        // Act
        sut.DownloadSave(save);

        // Assert
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Newer Local game save was found, downloading game save skipped!!")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_CopiesOverSaves_When_CloudSaveIsNewer(
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Cloud);

        // Act
        sut.DownloadSave(save);

        // Assert
        saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath), Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_RestoresLocalBackup_When_DownloadFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<IBackupManager> backupManagerMock,
        GameSave save,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath)).Throws(new Exception("Download error"));

        // Act
        var act = new Action(() => sut.DownloadSave(save));

        // Assert
        Assert.Throws<Exception>(act);
        backupManagerMock.Verify(x => x.RestoreBackup(save.LocalPath, save.GameName, save.SaveName, SaveType.Local), Times.Once);
    }

    [Theory, AutoMoqData]
    public void DownloadSave_LogsError_When_DownloadFails(
        [Frozen] Mock<ISaveCopier> saveCopierMock,
        [Frozen] Mock<ILogger<GameSaveSyncManager>> loggerMock,
        [Frozen] Mock<ILatestSaveTypeProvider> latestSaveTypeProviderMock,
        GameSave save,
        Exception exception,
        GameSaveSyncManager sut)
    {
        // Arrange
        saveCopierMock.Setup(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(exception);
        latestSaveTypeProviderMock.Setup(x => x.Get(save)).Returns(SaveType.Cloud);

        // Act
        var act = new Action(() => sut.DownloadSave(save));

        // Assert
        Assert.Throws<Exception>(act);
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Download failed.")),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}