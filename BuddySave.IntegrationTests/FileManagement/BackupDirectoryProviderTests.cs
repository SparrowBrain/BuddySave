using AutoFixture.Xunit2;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.System;
using BuddySave.TestTools;
using Moq;
using System;
using System.IO;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement;

public class BackupDirectoryProviderTests
{
    [Theory, AutoMoqData]
    public void GetTimestampedDirectory_ReturnsTimestampedDirectory_When_Called(
        string gameName,
        string saveName,
        SaveType saveType,
        DateTime now,
        [Frozen] Mock<IDateTimeProvider> dateTimeProviderMock,
        BackupDirectoryProvider sut)
    {
        // Arrange
        dateTimeProviderMock.Setup(x => x.Now()).Returns(now);
        var expectedResult = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavesBackup", gameName, saveName, saveType.ToString(), $"{now:yyyyMMdd_HHmmss}");

        // Act
        var result = sut.GetTimestampedDirectory(gameName, saveName, SaveType.Cloud);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory, AutoMoqData]
    public void GetRootDirectory_ReturnsRootGameSaveBackupsDirectory_When_Called(
        string gameName,
        string saveName,
        SaveType saveType,
        BackupDirectoryProvider sut)
    {
        // Arrange
        var expectedResult = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavesBackup", gameName, saveName, saveType.ToString());

        // Act
        var result = sut.GetRootDirectory(gameName, saveName, saveType);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}