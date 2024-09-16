using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.TestTools;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class GamingSessionTests
{
	[Theory]
	[AutoMoqData]
	public async Task Play_StartsClient_WhenLockExists(
		[Frozen] Mock<ILockManager> lockManagerMock,
		[Frozen] Mock<IClientSession> clientSessionMock,
		[Frozen] Mock<IServerSession> serverSessionMock,
		GameSave gameSave,
		Session ourSession,
		Session lockSession,
		ServerParameters serverParameters,
		ClientParameters clientParameters,
		GamingSession sut)
	{
		// Arrange
		lockManagerMock.Setup(x => x.LockExists(gameSave)).Returns(true);
		lockManagerMock.Setup(x => x.GetLockedSession(gameSave)).ReturnsAsync(lockSession);

		// Act
		await sut.Play(gameSave, ourSession, serverParameters, clientParameters);

		// Assert
		clientSessionMock.Verify(x => x.StartClient(lockSession, clientParameters), Times.Once());
		serverSessionMock.Verify(
			x => x.RunServerWithAutoSave(It.IsAny<GameSave>(), It.IsAny<Session>(), It.IsAny<ServerParameters>()),
			Times.Never);
	}

	[Theory]
	[AutoMoqData]
	public async Task Play_StartsServerAndClient_WhenLockDoesNotExist(
		[Frozen] Mock<ILockManager> lockManagerMock,
		[Frozen] Mock<IClientSession> clientSessionMock,
		[Frozen] Mock<IServerSession> serverSessionMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ClientParameters clientParameters,
		GamingSession sut)
	{
		// Arrange
		lockManagerMock.Setup(x => x.LockExists(gameSave)).Returns(false);

		// Act
		await sut.Play(gameSave, session, serverParameters, clientParameters);

		// Assert
		serverSessionMock.Verify(x => x.RunServerWithAutoSave(gameSave, session, serverParameters), Times.Once());
		clientSessionMock.Verify(
			x => x.StartClient(It.Is<Session>(s => s.Ip == "127.0.0.1" && s.Port == session.Port), clientParameters),
			Times.Once());
	}
}