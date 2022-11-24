using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.Notifications;
using BuddySave.TestTools;
using Moq;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class SharedSaveOrchestratorTests
{
    [Theory, AutoMoqData]
    public async Task Load_NotifyClient_When_GameSaveIsLocked(
        GameSave gameSave, 
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);

        // Act
        await sut.Load(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save is locked, your friends are playing!"), Times.Once);
        lockManagerMock.Verify(x => x.CreateLock(It.IsAny<GameSave>()), Times.Never);
        gameSaveSyncManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task Load_CallDeleteLock_When_CreateLockThrowsException(
        GameSave gameSave, 
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.CreateLock(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        await sut.Load(gameSave);

        // Assert
        lockManagerMock.Verify(x => x.DeleteLock(gameSave), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Failed loading game save. Deleting game save lock..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_CallDeleteLock_When_DownloadThrowsException(
        GameSave gameSave, 
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        gameSaveSyncManagerMock.Setup(x => x.DownloadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        await sut.Load(gameSave);

        // Assert
        lockManagerMock.Verify(x => x.DeleteLock(gameSave), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Failed loading game save. Deleting game save lock..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task Load_CreateLockFile_When_GameSaveIsNotLocked(
        GameSave gameSave, 
        [Frozen] Mock<ILockManager> lockManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        await sut.Load(gameSave);

        // Assert
        lockManagerMock.Verify(x => x.CreateLock(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_DownloadCloudSave_When_GameIsNotLocked(
        GameSave gameSave, 
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        await sut.Load(gameSave);

        // Assert
        gameSaveSyncManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save is prepared! Enjoy Buddy :)"), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_NotifyClient_When_LockFileDoesNotExist(
        GameSave gameSave, 
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        sut.Save(gameSave);
        
        // Assert
        clientNotifierMock.Verify(x => x.Notify("There's no lock. Cannot save."));
        gameSaveSyncManagerMock.Verify(x => x.UploadSave(It.IsAny<GameSave>()), Times.Never);
        lockManagerMock.Verify(x => x.DeleteLock(It.IsAny<GameSave>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public void Save_UploadSaveToCloud_When_LockExists(
        GameSave gameSave, 
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);

        // Act
        sut.Save(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Uploading game save to cloud..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save uploaded."), Times.Once);
        gameSaveSyncManagerMock.Verify(x => x.UploadSave(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_DeleteLockFile_When_LockExists(
        GameSave gameSave, 
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);
        
        // Act
        sut.Save(gameSave);
        
        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
        lockManagerMock.Verify(x => x.DeleteLock(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_DeleteLockFile_When_UploadThrowsException(
        GameSave gameSave,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        sut.Save(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
        lockManagerMock.Verify(x => x.DeleteLock(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_NotifyClient_When_UploadFails(
        GameSave gameSave,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws<Exception>();
        
        // Act
        sut.Save(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Upload failed."), Times.Once);
    }
}