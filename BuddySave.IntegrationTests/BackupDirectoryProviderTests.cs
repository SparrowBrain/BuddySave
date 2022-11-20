using BuddySave.TestTools;
using Xunit;

namespace BuddySave.IntegrationTests;

public class BackupDirectoryProviderTests
{
    [Theory, AutoMoqData]
    public void ReturnExpectedDirectory_When_Called(
        string saveName,
        BackupDirectoryProvider sut)
    {
        // Arrange
        var expectedResult = "SavesBackup" + saveName + SaveType.Cloud;
        
        // Act
        var result = sut.Get(saveName, SaveType.Cloud);
        
        // Assert
        Assert.Equal(expectedResult, result);
    }
}