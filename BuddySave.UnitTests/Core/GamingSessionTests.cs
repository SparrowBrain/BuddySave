﻿using AutoFixture.Xunit2;
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
		clientSessionMock.Verify(x => x.RunClient(lockSession, clientParameters), Times.Once());
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
		clientSessionMock.Verify(x => x.RunClient(session, clientParameters), Times.Once());
	}
}