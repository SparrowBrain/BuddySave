using BuddySave.TestTools;
using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Moq;
using Xunit;

namespace BuddySave.IntegrationTests
{
    public class CloudManagerTests
    {
        [Theory, AutoMoqData]
        public async Task UploadSave_DeletesOldCloudSave_WhenCloudSaveExists(
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, oldSavePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var backupSave = PrepareBackupDirectory(gameSave, backupDirectoryPath, backupDirectoryProviderMock, false);

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.False(Directory.Exists(oldSavePath));
            DisposeTempSaves(cloudSave, localSave, backupSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_CopiesSaveFolderLocalToCloud_WhenCloudSaveDoesNotExist(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var cloudSave = PrepareSaveDirectory(gameSave, SaveType.Cloud, false);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.True(Directory.Exists(gameSave.CloudPath));
            DisposeTempSaves(cloudSave, localSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_CopiesFilesToCloud_WhenLocalHasFiles(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var cloudSave = PrepareSaveDirectory(gameSave, SaveType.Cloud, false);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var cloudFilePath = Path.Combine(cloudSave.Path, file);

            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.True(File.Exists(cloudFilePath));
            DisposeTempSaves(cloudSave, localSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_LocalDoesNotExist_ThrowsAndKeepsCloud(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, cloudFilePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);

            // Act
            var act = new Action(() => sut.UploadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(cloudFilePath));
            DisposeTempSaves(cloudSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_LocalIsEmpty_ThrowsAndKeepsCloud(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, cloudFilePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var localSave = PrepareSaveDirectory(gameSave, SaveType.Local, true);

            // Act
            var act = new Action(() => sut.UploadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(cloudFilePath));
            DisposeTempSaves(cloudSave, localSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_BackupsCloudSaves_When_CloudSavesExistAndBackupDoesNot(
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var backupSave = PrepareBackupDirectory(gameSave, backupDirectoryPath, backupDirectoryProviderMock, false);
            var backupFilePath = Path.Combine(backupSave.Path, file);
            
            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.True(File.Exists(backupFilePath));
            DisposeTempSaves(cloudSave, localSave, backupSave);
        }

        [Theory, AutoMoqData]
        public async Task UploadSave_OverridesCloudOldBackupWithNewer_When_CloudSavesExistAndBackupAlreadyExist(
            string oldFile,
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var (backupSave, backupFilePathOld) = await PrepareBackupDirectoryWithFile(gameSave, backupDirectoryPath, backupDirectoryProviderMock, oldFile);
            
            // Act
            sut.UploadSave(gameSave);

            // Assert
            Assert.False(File.Exists(backupFilePathOld));
            DisposeTempSaves(cloudSave, localSave, backupSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_DeletesOldLocalSave_WhenLocalSaveExists(
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, localFilePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var backupSave = PrepareBackupDirectory(gameSave, backupDirectoryPath, backupDirectoryProviderMock, false);

            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.False(Directory.Exists(localFilePath));
            DisposeTempSaves(cloudSave, localSave, backupSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CopiesSaveFolderCloudToLocal_WhenLocalSaveDoesNotExist(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var localSave = PrepareSaveDirectory(gameSave, SaveType.Local, false);
            
            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.True(Directory.Exists(gameSave.LocalPath));
            DisposeTempSaves(cloudSave, localSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CopiesFilesToLocal_WhenCloudHasFiles(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var localSave = PrepareSaveDirectory(gameSave, SaveType.Local, false);
            var localFilePath = Path.Combine(localSave.Path, file);

            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.True(File.Exists(localFilePath));
            DisposeTempSaves(cloudSave, localSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CloudDoesNotExist_ThrowsAndKeepsLocal(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (localSave, localFilePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);

            // Act
            var act = new Action(() => sut.DownloadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(localFilePath));
            DisposeTempSaves(localSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_CloudIsEmpty_ThrowsAndKeepsLocal(
            string file,
            GameSave gameSave,
            CloudManager sut)
        {
            // Arrange
            var (localSave, localFilePath) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var cloudSave = PrepareSaveDirectory(gameSave, SaveType.Cloud, true);

            // Act
            var act = new Action(() => sut.DownloadSave(gameSave));

            // Assert
            Assert.ThrowsAny<Exception>(act);
            Assert.True(File.Exists(localFilePath));
            DisposeTempSaves(localSave, cloudSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_BackupsLocalSaves_When_CloudSavesExistAndBackupDoesNot(
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var backupSave = PrepareBackupDirectory(gameSave, backupDirectoryPath, backupDirectoryProviderMock, false);
            var backupFilePath = Path.Combine(backupSave.Path, file);
            
            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.True(File.Exists(backupFilePath));
            DisposeTempSaves(cloudSave, localSave, backupSave);
        }

        [Theory, AutoMoqData]
        public async Task DownloadSave_OverridesOldLocalBackupWithNewer_When_CloudSavesExistAndBackupAlreadyExist(
            string oldFile,
            string file,
            string backupDirectoryPath,
            GameSave gameSave,
            [Frozen] Mock<IBackupDirectoryProvider> backupDirectoryProviderMock,
            CloudManager sut)
        {
            // Arrange
            var (cloudSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Cloud, file);
            var (localSave, _) = await PrepareSaveDirectoryWithFile(gameSave, SaveType.Local, file);
            var (backupSave, backupFilePathOld) = await PrepareBackupDirectoryWithFile(gameSave, backupDirectoryPath, backupDirectoryProviderMock, oldFile);
            
            // Act
            sut.DownloadSave(gameSave);

            // Assert
            Assert.False(File.Exists(backupFilePathOld));
            DisposeTempSaves(cloudSave, localSave, backupSave);
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

        private static TempSaveDir PrepareSaveDirectory(GameSave gameSave, SaveType saveType, bool createDirectory)
        {
            var dirName = saveType == SaveType.Cloud ? gameSave.CloudPath : gameSave.LocalPath;
            var save = new TempSaveDir(dirName);

            if (saveType == SaveType.Cloud)
            {
                gameSave.CloudPath = save.Path;
            } 
            else
            {
                gameSave.LocalPath = save.Path;
            }

            if (createDirectory)
            {
                save.Create();
            }
            
            return save;
        }

        private static async Task<(TempSaveDir, string)> PrepareSaveDirectoryWithFile(GameSave gameSave, SaveType saveType, string file)
        {
            var save = PrepareSaveDirectory(gameSave, saveType, true);
            var filePath = Path.Combine(save.Path, file);
            await File.WriteAllTextAsync(filePath, "Test");
            return (save, filePath);
        }

        private static TempSaveDir PrepareBackupDirectory(GameSave gameSave, string backupDirectoryPath, Mock<IBackupDirectoryProvider> backupDirectoryProviderMock, bool createDirectory)
        {
            var backupSave = new TempSaveDir(backupDirectoryPath);
            backupDirectoryProviderMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<SaveType>())).Returns(backupSave.Path);

            if (createDirectory)
            {
                backupSave.Create();
            }
            
            return backupSave;
        }
        
        private static async Task<(TempSaveDir, string)> PrepareBackupDirectoryWithFile(GameSave gameSave, string backupDirectoryPath, Mock<IBackupDirectoryProvider> backupDirectoryProviderMock, string file)
        {
            var backupSave = PrepareBackupDirectory(gameSave, backupDirectoryPath, backupDirectoryProviderMock, true);
            var filePath = Path.Combine(backupSave.Path, file);
            await File.WriteAllTextAsync(filePath, "backup test");
            return (backupSave, filePath);
        }

        private static void DisposeTempSaves(params TempSaveDir[] saves)
        {
            foreach (var save in saves)
            {
                save.Dispose();
            }
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