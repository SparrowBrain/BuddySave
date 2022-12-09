using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.Notifications;
using BuddySave.TestTools;
using Moq;
using NLog;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class SharedSaveOrchestratorTests
{
    [Theory, AutoMoqData]
    public async Task Load_NotifyClient_When_GameSaveIsLocked(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save is locked, your friends are playing!"), Times.Once);
        lockManagerMock.Verify(x => x.CreateLock(It.IsAny<GameSave>(), It.IsAny<Session>()), Times.Never);
        gameSaveSyncManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task Load_CallDeleteLock_When_CreateLockThrowsException(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.CreateLock(It.IsAny<GameSave>(), It.IsAny<Session>())).Throws<Exception>();

        // Act
        await sut.Load(gameSave, session);

        // Assert
        lockManagerMock.Verify(x => x.DeleteLock(gameSave, session), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Failed loading game save. Deleting game save lock..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_CallDeleteLock_When_DownloadThrowsException(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        gameSaveSyncManagerMock.Setup(x => x.DownloadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        await sut.Load(gameSave, session);

        // Assert
        lockManagerMock.Verify(x => x.DeleteLock(gameSave, session), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Failed loading game save. Deleting game save lock..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_LogException_When_DownloadThrowsException(
        GameSave gameSave,
        Session session,
        Exception exception,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILogger> loggerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        gameSaveSyncManagerMock.Setup(x => x.DownloadSave(It.IsAny<GameSave>())).Throws(exception);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        loggerMock.Verify(x => x.Error(exception, "Error while loading."));
    }

    [Theory, AutoMoqData]
    public async Task Load_CreateLockFile_When_GameSaveIsNotLocked(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(false);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        lockManagerMock.Verify(x => x.CreateLock(gameSave, session), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_DownloadCloudSave_When_GameIsNotLocked(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(false);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        gameSaveSyncManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save is prepared! Enjoy Buddy :)"), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_NotifyClient_When_LockFileDoesNotExist(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(false);

        // Act
        await sut.Save(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify($"You don't have a lock on a {gameSave.GameName}, cannot save."));
        gameSaveSyncManagerMock.Verify(x => x.UploadSave(It.IsAny<GameSave>()), Times.Never);
        lockManagerMock.Verify(x => x.DeleteLock(It.IsAny<GameSave>(), It.IsAny<Session>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task Save_UploadSaveToCloud_When_LockExists(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);

        // Act
        await sut.Save(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Uploading game save to cloud..."), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save uploaded."), Times.Once);
        gameSaveSyncManagerMock.Verify(x => x.UploadSave(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_DeleteLockFile_When_LockExists(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);

        // Act
        await sut.Save(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
        lockManagerMock.Verify(x => x.DeleteLock(gameSave, session), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_DeleteLockFile_When_UploadThrowsException(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        await sut.Save(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
        lockManagerMock.Verify(x => x.DeleteLock(gameSave, session), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_NotifyClient_When_UploadFails(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        await sut.Save(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Upload failed."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_LogError_When_UploadThrowsException(
        GameSave gameSave,
        Session session,
        Exception exception,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<ILogger> loggerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws(exception);

        // Act
        await sut.Save(gameSave, session);

        // Assert
        loggerMock.Verify(x => x.Error(exception, "Error while saving."));
    }
}