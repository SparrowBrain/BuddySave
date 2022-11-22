using AutoFixture.Xunit2;
using BuddySave.TestTools;
using Moq;
using System;
using Xunit;

namespace BuddySave.UnitTests
{
    public class CloudManagerTests
    {
        [Theory, AutoMoqData]
        public void UploadSave_Throws_When_SourceValidationFails(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            // Act
            var act = new Action(() => sut.UploadSave(save));

            // Assert
            Assert.ThrowsAny<Exception>(act);
        }

        [Theory, AutoMoqData]
        public void UploadSave_DoesNotBackup_When_CloudHasNoSave(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.ValidateSource(save.Name, save.CloudPath)).Throws(new Exception());

            // Act
            sut.UploadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.CloudPath, It.IsAny<string>()), Times.Never());
        }

        [Theory, AutoMoqData]
        public void UploadSave_BacksUpCloud_When_CloudHasASave(
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            string backupDir,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), SaveType.Cloud)).Returns(backupDir);

            // Act
            sut.UploadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.CloudPath, backupDir), Times.Once());
        }

        [Theory, AutoMoqData]
        public void UploadSave_CopiesLocalSavesToCloud_When_LocalSaveIsValid(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            GameSave save,
            CloudManager sut)
        {
            // Act
            sut.UploadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.LocalPath, save.CloudPath), Times.Once());
        }

        [Theory, AutoMoqData]
        public void UploadSave_RestoresCloudBackup_When_UploadFails(
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            string backupDir,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), SaveType.Cloud)).Returns(backupDir);
            saveCopierMock.Setup(x => x.CopyOverSaves(save.Name, save.LocalPath, save.CloudPath)).Throws(new Exception("Upload error"));

            // Act
            sut.UploadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, backupDir, save.CloudPath), Times.Once());
        }

        [Theory, AutoMoqData]
        public void DownloadSave_Throws_When_SourceValidationFails(
           [Frozen] Mock<ISaveCopier> saveCopierMock,
           GameSave save,
           CloudManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.ValidateSource(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            // Act
            var act = new Action(() => sut.DownloadSave(save));

            // Assert
            Assert.ThrowsAny<Exception>(act);
        }

        [Theory, AutoMoqData]
        public void DownloadSave_DoesNotBackup_When_LocalHasNoSave(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            saveCopierMock.Setup(x => x.ValidateSource(save.Name, save.LocalPath)).Throws(new Exception());

            // Act
            sut.DownloadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.LocalPath, It.IsAny<string>()), Times.Never());
        }

        [Theory, AutoMoqData]
        public void DownloadSave_BacksUpLocal_When_LocalHasASave(
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            string backupDir,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), SaveType.Local)).Returns(backupDir);

            // Act
            sut.DownloadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.LocalPath, backupDir), Times.Once());
        }

        [Theory, AutoMoqData]
        public void DownloadSave_CopiesCloudSavesToLocal_When_CloudSaveIsValid(
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            GameSave save,
            CloudManager sut)
        {
            // Act
            sut.DownloadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, save.CloudPath, save.LocalPath), Times.Once());
        }

        [Theory, AutoMoqData]
        public void DownloadSave_RestoresLocalBackup_When_DownloadFails(
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            string backupDir,
            GameSave save,
            CloudManager sut)
        {
            // Arrange
            backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), SaveType.Local)).Returns(backupDir);
            saveCopierMock.Setup(x => x.CopyOverSaves(save.Name, save.CloudPath, save.LocalPath)).Throws(new Exception("Download error"));

            // Act
            sut.DownloadSave(save);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(save.Name, backupDir, save.LocalPath), Times.Once());
        }
    }
}