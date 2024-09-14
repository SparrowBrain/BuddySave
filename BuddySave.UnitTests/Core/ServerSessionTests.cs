using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.System;
using BuddySave.TestTools;
using Moq;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class ServerSessionTests
{
	[Theory]
	[InlineAutoMoqData((string)null)]
	[InlineAutoMoqData("")]
	public async Task Run_ThrowsException_WhenNoServerPathIsEmpty(
		string serverPath,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		serverParameters.Path = serverPath;

		// Act
		var act = new Func<Task>(() => sut.RunServerWithAutoSave(gameSave, session, serverParameters));

		// Assert
		await Assert.ThrowsAsync<ArgumentException>(act);
	}

	[Theory]
	[AutoMoqData]
	public async Task Run_DoesNotStartTheServer_WhenLoadingSaveFails(
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		[Frozen] Mock<IProcessProvider> processProviderMock,
		Exception exception,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>())).Throws(exception);

		// Act
		var act = new Func<Task>(() => sut.RunServerWithAutoSave(gameSave, session, serverParameters));

		// Assert
		await Assert.ThrowsAnyAsync<Exception>(act);
		processProviderMock.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Never());
	}

	[Theory]
	[InlineAutoMoqData(OrchestratorResult.SaveLocked)]
	[InlineAutoMoqData(OrchestratorResult.Failed)]
	public async Task Run_DoesNotStartTheServer_WhenGameSaveIsNotLoaded(
		OrchestratorResult orchestratorResult,
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		[Frozen] Mock<IProcessProvider> processProviderMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(orchestratorResult);

		// Act
		await sut.RunServerWithAutoSave(gameSave, session, serverParameters);

		// Assert
		processProviderMock.Verify(x => x.Start(It.IsAny<ProcessStartInfo>()), Times.Never());
	}

	[Theory]
	[InlineAutoMoqData(OrchestratorResult.SaveLocked)]
	[InlineAutoMoqData(OrchestratorResult.Failed)]
	public async Task Run_DoesWaitForServerToStop_WhenGameSaveIsNotLoaded(
		OrchestratorResult orchestratorResult,
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		[Frozen] Mock<IProcessProvider> processProviderMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(orchestratorResult);

		// Act
		await sut.RunServerWithAutoSave(gameSave, session, serverParameters);

		// Assert
		processProviderMock.Verify(x => x.WaitForExitAsync(It.IsAny<Process>()), Times.Never());
	}

	[Theory]
	[InlineAutoMoqData(OrchestratorResult.SaveLocked)]
	[InlineAutoMoqData(OrchestratorResult.Failed)]
	public async Task Run_DoesNotCallSave_WhenGameSaveIsNotLoaded(
		OrchestratorResult orchestratorResult,
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>())).ReturnsAsync(orchestratorResult);

		// Act
		await sut.RunServerWithAutoSave(gameSave, session, serverParameters);

		// Assert
		sharedSaveOrchestratorMock.Verify(x => x.Save(It.IsAny<GameSave>(), It.IsAny<Session>()), Times.Never());
	}

	[Theory]
	[AutoMoqData]
	public async Task Run_DoesNotSave_WhenStartingServerFails(
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		[Frozen] Mock<IProcessProvider> processProviderMock,
		Exception exception,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock
			.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>()))
			.ReturnsAsync(OrchestratorResult.Loaded);
		processProviderMock.Setup(x => x.Start(It.IsAny<ProcessStartInfo>())).Throws(exception);

		// Act
		var act = new Func<Task>(() => sut.RunServerWithAutoSave(gameSave, session, serverParameters));

		// Assert
		await Assert.ThrowsAnyAsync<Exception>(act);
		sharedSaveOrchestratorMock.Verify(x => x.Save(It.IsAny<GameSave>(), It.IsAny<Session>()), Times.Never());
	}

	[Theory]
	[AutoMoqData]
	public async Task Run_WaitsForServerStop_WhenServerStarts(
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		[Frozen] Mock<IProcessProvider> processProviderMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock
			.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>()))
			.ReturnsAsync(OrchestratorResult.Loaded);

		// Act
		await sut.RunServerWithAutoSave(gameSave, session, serverParameters);

		// Assert
		processProviderMock.Verify(x => x.WaitForExitAsync(It.IsAny<Process>()));
	}

	[Theory]
	[AutoMoqData]
	public async Task Run_Saves_WhenServerStops(
		[Frozen] Mock<ISharedSaveOrchestrator> sharedSaveOrchestratorMock,
		GameSave gameSave,
		Session session,
		ServerParameters serverParameters,
		ServerSession sut)
	{
		// Arrange
		sharedSaveOrchestratorMock
			.Setup(x => x.Load(It.IsAny<GameSave>(), It.IsAny<Session>()))
			.ReturnsAsync(OrchestratorResult.Loaded);

		// Act
		await sut.RunServerWithAutoSave(gameSave, session, serverParameters);

		// Assert
		sharedSaveOrchestratorMock.Verify(x => x.Save(It.IsAny<GameSave>(), It.IsAny<Session>()));
	}
}