using BuddySave.TestTools;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace BuddySave.IntegrationTests
{
    public class CloudManagerTests
    {
        [Theory, AutoMoqData]
        public async Task UploadSave_DeletesOldCloudSave_WhenCloudSaveExists(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            gameSave.CloudPath = cloudSave.Path;
            gameSave.LocalPath = localSave.Path;

            cloudSave.Create();
            localSave.Create();

            var localPath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(localPath, "Test");
            var oldSavePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(oldSavePath, "Test");

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.False(Directory.Exists(oldSavePath));
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_CopiesSaveFolderLocalToCloud_WhenCloudSaveDoesNotExist(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            gameSave.CloudPath = cloudSave.Path;
            gameSave.LocalPath = localSave.Path;

            localSave.Create();

            var localPath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(localPath, "Test");

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.True(Directory.Exists(gameSave.CloudPath));
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_CopiesFilesToCloud_WhenLocalHasFiles(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            gameSave.CloudPath = cloudSave.Path;
            gameSave.LocalPath = localSave.Path;

            localSave.Create();
            var localFilePath = Path.Combine(localSave.Path, file);
            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(localFilePath, "Test");

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.True(File.Exists(cloudFilePath));
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_LocalDoesNotExist_ThrowsAndKeepsCloud(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            gameSave.CloudPath = cloudSave.Path;

            cloudSave.Create();
            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(cloudFilePath, "Test");

            // Act
            var act = new Action(() => sut.UploadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(cloudFilePath));
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_LocalIsEmpty_ThrowsAndKeepsCloud(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            gameSave.CloudPath = cloudSave.Path;
            gameSave.LocalPath = localSave.Path;

            cloudSave.Create();
            localSave.Create();
            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(cloudFilePath, "Test");

            // Act
            var act = new Action(() => sut.UploadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(cloudFilePath));
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_DeletesOldLocalSave_WhenLocalSaveExists(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            gameSave.LocalPath = localSave.Path;
            gameSave.CloudPath = cloudSave.Path;

            localSave.Create();
            cloudSave.Create();

            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(cloudFilePath, "Test");
            var oldSavePath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(oldSavePath, "Test");

            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.False(Directory.Exists(oldSavePath));
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CopiesSaveFolderCloudToLocal_WhenLocalSaveDoesNotExist(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            gameSave.LocalPath = localSave.Path;
            gameSave.CloudPath = cloudSave.Path;

            cloudSave.Create();

            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            await File.WriteAllTextAsync(cloudFilePath, "Test");

            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.True(Directory.Exists(gameSave.LocalPath));
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CopiesFilesToLocal_WhenCloudHasFiles(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            gameSave.LocalPath = localSave.Path;
            gameSave.CloudPath = cloudSave.Path;

            cloudSave.Create();
            var cloudFilePath = Path.Combine(cloudSave.Path, file);
            var localFilePath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(cloudFilePath, "Test");

            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.True(File.Exists(localFilePath));
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CloudDoesNotExist_ThrowsAndKeepsLocal(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            gameSave.LocalPath = localSave.Path;

            localSave.Create();
            var localFilePath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(localFilePath, "Test");

            // Act
            var act = new Action(() => sut.DownloadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(localFilePath));
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CloudIsEmpty_ThrowsAndKeepsLocal(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            using var localSave = new TempSaveDir(gameSave.LocalPath);
            using var cloudSave = new TempSaveDir(gameSave.CloudPath);
            gameSave.LocalPath = localSave.Path;
            gameSave.CloudPath = cloudSave.Path;

            localSave.Create();
            cloudSave.Create();
            var localFilePath = Path.Combine(localSave.Path, file);
            await File.WriteAllTextAsync(localFilePath, "Test");

            // Act
            var act = new Action(() => sut.DownloadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(localFilePath));
        }

        [Theory, AutoMoqData]
        public void LockExists_ReturnsFalse_WhenNoLockFile(
            GameSave gameSave,
            CloudManager sut)
        {
            // Act
            var result = sut.LockExists(gameSave);

            // Assert
            Assert.False(result);
        }

        [Theory, AutoMoqData]
        public async Task LockExists_ReturnsTrue_WhenLockFileExists(
            GameSave gameSave,
            CloudManager sut)
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
            CloudManager sut)
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
            CloudManager sut)
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
            CloudManager sut)
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
            CloudManager sut)
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

        private class TempSaveDir : IDisposable
        {
            public TempSaveDir(string dirName)
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), dirName);
            }

            public string Path { get; }

            public void Dispose()
            {
                Directory.Delete(Path, true);
            }

            public void Create()
            {
                Directory.CreateDirectory(Path);
            }
        }
    }
}