using System.Threading.Tasks;
using AutoFixture.Xunit2;
using BuddySave.TestTools;
using Moq;
using Xunit;

namespace BuddySave.UnitTests;

public class SharedSaveOrchestratorTests
{
    [Theory, AutoMoqData]
    public async Task Load_NotifyClient_When_GameSaveIsLocked(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);

        // Act
        await sut.Load(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save is locked, your friends are playing!"), Times.Once);
        cloudManagerMock.Verify(x => x.CreateLock(It.IsAny<GameSave>()), Times.Never);
        cloudManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Never);
    }

    [Theory, AutoMoqData]
    public async Task Load_CreateLockFile_When_GameSaveIsNotLocked(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        await sut.Load(gameSave);

        // Assert
        cloudManagerMock.Verify(x => x.CreateLock(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Load_DownloadCloudSave_When_GameIsNotLocked(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        await sut.Load(gameSave);

        // Assert
        cloudManagerMock.Verify(x => x.DownloadSave(gameSave), Times.Once);
        clientNotifierMock.Verify(x => x.Notify("Game save is prepared! Enjoy Buddy :)"), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_NotifyClient_When_LockFileDoesNotExist(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(false);

        // Act
        sut.Save(gameSave);
        
        // Assert
        clientNotifierMock.Verify(x => x.Notify("There's no lock. Cannot save."));
        cloudManagerMock.Verify(x => x.UploadSave(It.IsAny<GameSave>()), Times.Never);
        cloudManagerMock.Verify(x => x.DeleteLock(It.IsAny<GameSave>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public void Save_UploadSaveToCloud_When_LockExists(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);

        // Act
        sut.Save(gameSave);

        // Assert
        clientNotifierMock.Verify(x => x.Notify("Uploading game save to cloud..."), Times.Once);
        cloudManagerMock.Verify(x => x.UploadSave(gameSave), Times.Once);
    }

    [Theory, AutoMoqData]
    public void Save_DeleteLockFile_When_LockExists(
        GameSave gameSave, 
        [Frozen] Mock<ICloudManager> cloudManagerMock,
        [Frozen] Mock<IClientNotifier> clientNotifierMock,
        SharedSaveOrchestrator sut)
    {
        // Arrange
        cloudManagerMock.Setup(x => x.LockExists(It.IsAny<GameSave>())).Returns(true);
        
        // Act
        sut.Save(gameSave);
        
        // Assert
        clientNotifierMock.Verify(x => x.Notify("Game save lock released."), Times.Once);
        cloudManagerMock.Verify(x => x.DeleteLock(gameSave), Times.Once);
    }
}