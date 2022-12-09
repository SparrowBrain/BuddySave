using System;
using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Moq;
using NLog;
using Xunit;

namespace BuddySave.UnitTests.Core
{
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
            GameSave save,
            GameSaveSyncManager sut)
        {
            // Act
            sut.UploadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.SaveName, save.LocalPath, save.CloudPath), Times.Once);
        }

        [Theory, AutoMoqData]
        public void UploadSave_RestoresCloudBackup_When_UploadFails(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            [Frozen] Mock<IBackupManager> backupManagerMock,
            GameSave save,
            GameSaveSyncManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            // Act
            sut.UploadSave(save);

            // Assert
            backupManagerMock.Verify(x => x.RestoreBackup(save.CloudPath, save.GameName, save.SaveName, SaveType.Cloud), Times.Once);
        }

        [Theory, AutoMoqData]
        public void UploadSave_LogsError_When_UploadFails(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            [Frozen] Mock<ILogger> loggerMock,
            GameSave save,
            Exception exception,
            GameSaveSyncManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.CopyOverSaves(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

            // Act
            sut.UploadSave(save);

            // Assert
            loggerMock.Verify(x => x.Error(exception, "Upload failed."));
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
        public void DownloadSave_RestoresLocalBackup_When_DownloadFails(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            [Frozen] Mock<IBackupManager> backupManagerMock,
            GameSave save,
            GameSaveSyncManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath)).Throws(new Exception("Download error"));

            // Act
            sut.DownloadSave(save);

            // Assert
            backupManagerMock.Verify(x => x.RestoreBackup(save.LocalPath, save.GameName, save.SaveName, SaveType.Local), Times.Once);
        }

        [Theory, AutoMoqData]
        public void DownloadSave_LogsError_When_DownloadFails(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            [Frozen] Mock<ILogger> loggerMock,
            GameSave save,
            Exception exception,
            GameSaveSyncManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.CopyOverSaves(save.SaveName, save.CloudPath, save.LocalPath)).Throws(exception);

            // Act
            sut.DownloadSave(save);

            // Assert
            loggerMock.Verify(x => x.Error(exception, "Download failed."));
        }
    }
}