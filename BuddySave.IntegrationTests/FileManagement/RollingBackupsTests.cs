using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Moq;
using System.IO;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement
{
    public class RollingBackupsTests
    {
        [Theory]
        [AutoMoqData]
        public void GetCount_ReturnsNumberOfExistingBackups(
            string gameName,
            string saveName,
            SaveType saveType,
            string[] saveDirectories,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            RollingBackups sut)
        {
            // Arrange
            using var tempDir = new TempDir(saveName, true);
            backupDirectoryProviderMock
                .Setup(x => x.GetRootDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>()))
                .Returns(tempDir.Path);

            foreach (var dir in saveDirectories)
            {
                Directory.CreateDirectory(Path.Combine(tempDir.Path, dir));
            }

            // Act
            var result = sut.GetCount(gameName, saveName, saveType);

            // Assert
            Assert.Equal(saveDirectories.Length, result);
        }

        [Theory]
        [AutoMoqData]
        public void GetCount_ReturnsZero_When_BackupDirectoryDoesNotExist(
            string gameName,
            string saveName,
            SaveType saveType,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            RollingBackups sut)
        {
            // Arrange
            using var tempDir = new TempDir(saveName, false);
            backupDirectoryProviderMock
                .Setup(x => x.GetRootDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>()))
                .Returns(tempDir.Path);

            // Act
            var result = sut.GetCount(gameName, saveName, saveType);

            // Assert
            Assert.Equal(0, result);
        }

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
        [AutoMoqData]
        public void DeleteOldest_DeletesOldestSave(
            string gameName,
            string saveName,
            SaveType saveType,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            RollingBackups sut)
        {
            // Arrange
            var saveDirectories = new[] { "20220101_010101", "20220101_010102" };
            using var tempDir = new TempDir(saveName, true);
            var deletedPath = Path.Combine(tempDir.Path, "20220101_010101");
            backupDirectoryProviderMock
                .Setup(x => x.GetRootDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SaveType>()))
                .Returns(tempDir.Path);

            foreach (var dir in saveDirectories)
            {
                Directory.CreateDirectory(Path.Combine(tempDir.Path, dir));
            }

            // Act
            sut.DeleteOldest(gameName, saveName, saveType);

            // Assert
            Assert.False(Directory.Exists(deletedPath));
        }
    }
}