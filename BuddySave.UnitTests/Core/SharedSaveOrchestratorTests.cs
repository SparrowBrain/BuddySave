using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.Notifications;
using BuddySave.TestTools;
using Microsoft.Extensions.Logging;
using Moq;
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
        lockManagerMock.Setup(x => x.GetLockedSession(It.IsAny<GameSave>())).ReturnsAsync(session);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save is locked, your friends are playing!"), Times.Once);
        clientNotifierMock.Verify(x => x.Notify($"Connect to {session.UserName}'s server using {session.Ip}:{session.Port}"), Times.Once);
        lockManagerMock.Verify(x => x.CreateLock(It.IsAny<GameSave>(), It.IsAny<Session>()), Times.Never);
        gameSaveSyncManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Never);
    }
    
    [Theory, AutoMoqData]
    public async Task Load_ReturnSaveLockedResult_When_GameSaveIsLocked(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);
        lockManagerMock.Setup(x => x.GetLockedSession(It.IsAny<GameSave>())).ReturnsAsync(session);

        // Act
        var result = await sut.Load(gameSave, session);

        // Assert
        Assert.Equal(OrchestratorResult.SaveLocked, result);
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
    public async Task Load_ReturnFailedResult_When_CreateLockThrowsException(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.CreateLock(It.IsAny<GameSave>(), It.IsAny<Session>())).Throws<Exception>();

        // Act
        var result = await sut.Load(gameSave, session);

        // Assert
        Assert.Equal(OrchestratorResult.Failed, result);
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
    public async Task Load_ReturnFailedResult_When_DownloadThrowsException(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        gameSaveSyncManagerMock.Setup(x => x.DownloadSave(It.IsAny<GameSave>())).Throws<Exception>();

        // Act
        var result = await sut.Load(gameSave, session);

        // Assert
        Assert.Equal(OrchestratorResult.Failed, result);
    }

    [Theory, AutoMoqData]
    public async Task Load_LogException_When_DownloadThrowsException(
        GameSave gameSave,
        Session session,
        Exception exception,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILogger<SharedSaveOrchestrator>> loggerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        gameSaveSyncManagerMock.Setup(x => x.DownloadSave(It.IsAny<GameSave>())).Throws(exception);

        // Act
        await sut.Load(gameSave, session);

        // Assert
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while loading.")),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true))
        );
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
    public async Task Load_ReturnLoadedResult_When_LoadIsSuccessful(
        GameSave gameSave,
        Session session,
        [Frozen] Mock<ILockManager> lockManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(false);

        // Act
        var result = await sut.Load(gameSave, session);

        // Assert
        Assert.Equal(OrchestratorResult.Loaded, result);
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
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);

        // Act
        await sut.Save(gameSave, session);

        // Assert
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
        clientNotifierMock.Verify(x => x.Notify("Error while saving."), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Save_LogError_When_UploadThrowsException(
        GameSave gameSave,
        Session session,
        Exception exception,
        [Frozen] Mock<IGameSaveSyncManager> gameSaveSyncManagerMock,
        [Frozen] Mock<ILockManager> lockManagerMock,
        [Frozen] Mock<ILogger<SharedSaveOrchestrator>> loggerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        lockManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(true);
        gameSaveSyncManagerMock.Setup(x => x.UploadSave(It.IsAny<GameSave>())).Throws(exception);

        // Act
        await sut.Save(gameSave, session);

        // Assert
        loggerMock.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error while saving.")),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true))
        );
    }
}