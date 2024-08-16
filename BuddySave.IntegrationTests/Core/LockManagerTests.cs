using System;
using System.IO;
using System.Threading.Tasks;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.TestTools;
using Xunit;

namespace BuddySave.IntegrationTests.Core;

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
    public async Task LockExistsForUserName_ReturnsFalse_WhenNoLockFile(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Act
        var result = await sut.LockExists(gameSave, session);

        // Assert
        Assert.False(result);
    }

    [Theory, AutoMoqData]
    public async Task LockExistsForUserName_ReturnsFalse_WhenSaveIsLockedByAnotherUserName(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        await lockFile.Create("buddy");
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        var result = await sut.LockExists(gameSave, session);

        // Assert
        Assert.False(result);
    }

    [Theory, AutoMoqData]
    public async Task LockExistsForUserName_ReturnsTrue_WhenSaveIsLockedByCurrentUser(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        await lockFile.Create(session.UserName);
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        var result = await sut.LockExists(gameSave, session);

        // Assert
        Assert.True(result);
    }

    [Theory, AutoMoqData]
    public async Task CreateLock_ThrowsException_WhenLockExists(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        await lockFile.Create();
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        var act = new Func<Task>(async () => await sut.CreateLock(gameSave, session));

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);
    }

    [Theory, AutoMoqData]
    public async Task CreateLock_CreatesLock_WhenLockDoesNotExist(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        await sut.CreateLock(gameSave, session);

        // Assert
        Assert.True(File.Exists(lockFile.LockPath));
        var expectedFileContent = (await File.ReadAllTextAsync(lockFile.LockPath)).Trim();
        Assert.Equal(expectedFileContent, session.UserName);
    }

    [Theory, AutoMoqData]
    public async Task DeleteLock_DeletesLock_WhenLockExists(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        await lockFile.Create(session.UserName);
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        await sut.DeleteLock(gameSave, session);

        // Assert
        Assert.False(File.Exists(lockFile.LockPath));
    }

    [Theory, AutoMoqData]
    public async Task DeleteLock_DoesNothing_WhenLockDoesNotExist(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        await sut.DeleteLock(gameSave, session);

        // Assert
        Assert.False(File.Exists(lockFile.LockPath));
    }

    [Theory, AutoMoqData]
    public async Task DeleteLock_ThrowsException_WhenLockIsCreatedByAnotherUser(
        GameSave gameSave,
        Session session,
        LockManager sut)
    {
        // Arrange
        using var lockFile = new TempLockFile();
        await lockFile.Create("buddyUser");
        gameSave.CloudPath = lockFile.CloudPath;

        // Act
        var act = new Func<Task>(async () => await sut.DeleteLock(gameSave, session));

        // Assert
        var exception = await Assert.ThrowsAsync<Exception>(act);
        Assert.Equal("Cannot delete lock. Lock is owned by buddyUser.", exception.Message);
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

        public async Task Create(string contents)
        {
            await File.WriteAllTextAsync(LockPath, contents);
        }
    }
}