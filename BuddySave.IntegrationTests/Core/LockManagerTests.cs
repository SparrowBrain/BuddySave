using System;
using System.IO;
using System.Threading.Tasks;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.TestTools;
using Xunit;

namespace BuddySave.IntegrationTests.Core
{
    public class LockManagerTests
    {
        [Theory, AutoMoqData]
        public void LockExists_ReturnsFalse_WhenNoLockFile(
            GameSave gameSave,
            LockManager sut)
        {
            // Act
            var result = sut.LockExists(gameSave);

            // Assert
            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task LockExists_ReturnsTrue_WhenLockFileExists(
            GameSave gameSave,
            LockManager sut)
        {
            // Arrange
            using var lockFile = new TempLockFile();
            await lockFile.Create();
            gameSave.CloudPath = lockFile.CloudPath;

            // Act
            var result = sut.LockExists(gameSave);

            // Assert
            Assert.True(result);
        }

        [Theory, AutoMoqData]
        public async Task CreateLock_ThrowsException_WhenLockExists(
            GameSave gameSave,
            LockManager sut)
        {
            // Arrange
            using var lockFile = new TempLockFile();
            await lockFile.Create();
            gameSave.CloudPath = lockFile.CloudPath;

            // Act
            var act = new Func<Task>(async () => await sut.CreateLock(gameSave));

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(act);
        }

        [Theory, AutoMoqData]
        public async Task CreateLock_CreatesLock_WhenLockDoesNotExist(
            GameSave gameSave,
            LockManager sut)
        {
            // Arrange
            using var lockFile = new TempLockFile();
            gameSave.CloudPath = lockFile.CloudPath;

            // Act
            await sut.CreateLock(gameSave);

            // Assert
            Assert.True(File.Exists(lockFile.LockPath));
        }

        [Theory, AutoMoqData]
        public async Task DeleteLock_DeletesLock_WhenLockExists(
            GameSave gameSave,
            LockManager sut)
        {
            // Arrange
            using var lockFile = new TempLockFile();
            await lockFile.Create();
            gameSave.CloudPath = lockFile.CloudPath;

            // Act
            sut.DeleteLock(gameSave);

            // Assert
            Assert.False(File.Exists(lockFile.LockPath));
        }

        [Theory, AutoMoqData]
        public void DeleteLock_DoesNothing_WhenLockDoesNotExist(
            GameSave gameSave,
            LockManager sut)
        {
            // Arrange
            using var lockFile = new TempLockFile();
            gameSave.CloudPath = lockFile.CloudPath;

            // Act
            sut.DeleteLock(gameSave);

            // Assert
            Assert.False(File.Exists(lockFile.LockPath));
        }

        private class TempLockFile : IDisposable
        {
            public TempLockFile()
            {
                CloudPath = Path.GetTempFileName();
                LockPath = CloudPath + ".lock";
            }

            public string LockPath { get; }

            public string CloudPath { get; }

            public void Dispose()
            {
                File.Delete(LockPath);
            }

            public async Task Create()
            {
                await File.WriteAllTextAsync(LockPath, "This is a test lock");
            }
        }
    }
}