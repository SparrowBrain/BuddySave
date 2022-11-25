using System;
using System.IO;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.TestTools;
using Xunit;

namespace BuddySave.IntegrationTests.FileManagement;

public class BackupDirectoryProviderTests
{
    [Theory, AutoMoqData]
    public void ReturnExpectedDirectory_When_Called(
        string saveName,
        BackupDirectoryProvider sut)
    {
        // Arrange
        var expectedResult = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"SavesBackup_{saveName}_{SaveType.Cloud}");
        
        // Act
        var result = sut.Get(saveName, SaveType.Cloud);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
}