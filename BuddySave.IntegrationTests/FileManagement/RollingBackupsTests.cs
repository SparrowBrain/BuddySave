using AutoFixture;
using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Moq;
using System.IO;
using System.Linq;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement
{
    public class RollingBackupsTests
    {
        [Theory]
        [AutoMoqData]
        public void GetMostRecent_ReturnsMostRecentSave(
            string gameName,
            string saveName,
            SaveType saveType,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            RollingBackups sut)
        {
            // Arrange
            var saveDirectories = new[] { "20220101_010101", "20220101_010102" };
            using var tempDir = new TempDir(saveName, true);
            var expectedDir = Path.Combine(tempDir.Path, "20220101_010102");
            backupDirectoryProviderMock
                .Setup(x => x.GetRootDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>()))
                .Returns(tempDir.Path);

            foreach (var dir in saveDirectories)
            {
                Directory.CreateDirectory(Path.Combine(tempDir.Path, dir));
            }

            // Act
            var result = sut.GetMostRecent(gameName, saveName, saveType);

            // Assert
            Assert.Equal(expectedDir, result);
        }

        [Theory]
        [InlineAutoMoqData(SaveType.Cloud)]
        [InlineAutoMoqData(SaveType.Local)]
        public void Add_CopiesSavesToTimestampedDirectory(
            SaveType saveType,
            string gameName,
            string saveName,
            string timestampedDir,
            string savePath,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            [Frozen] Mock<ISaveCopier> saveCopierMock,
            RollingBackups sut)
        {
            // Arrange
            backupDirectoryProviderMock.Setup(x => x.GetTimestampedDirectory(gameName, saveName, saveType))
                .Returns(timestampedDir);

            // Act
            sut.Add(savePath, gameName, saveName, saveType);

            // Assert
            saveCopierMock.Verify(x => x.CopyOverSaves(saveName, savePath, timestampedDir), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData(SaveType.Cloud)]
        [InlineAutoMoqData(SaveType.Local)]
        public void Add_DeletesOldestRollingBackup_When_MoreThanTenRollingBackupsExist(
            SaveType saveType,
            string gameName,
            string saveName,
            string savePath,
            Fixture fixture,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            RollingBackups sut)
        {
            // Arrange
            using var tempDir = new TempDir(saveName, true);
            backupDirectoryProviderMock
                .Setup(x => x.GetRootDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>()))
                .Returns(tempDir.Path);

            var backups = fixture.CreateMany<string>(11);
            foreach (var backup in backups)
            {
                Directory.CreateDirectory(Path.Combine(tempDir.Path, backup));
            }

            var deletedPath = Path.Combine(tempDir.Path, backups.OrderBy(x => x).First());

            // Act
            sut.Add(savePath, gameName, saveName, saveType);

            // Assert
            Assert.False(Directory.Exists(deletedPath));
        }
    }
}