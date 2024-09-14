using AutoFixture.Xunit2;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.System;
using BuddySave.TestTools;
using Moq;
using System;
using System.Diagnostics;
using Xunit;

namespace BuddySave.UnitTests.Core;

public class ClientSessionTests
{
	[Theory]
	[InlineAutoMoqData("")]
	[InlineAutoMoqData((string)null)]
	public void RunClient_ThrowsArgumentException_WhenClientPathIsNull(
		string path,
		Session session,
		ClientParameters clientParameters,
		ClientSession sut)
	{
		// Arrange
		clientParameters.Path = path;

		// Act
		var exception = Record.Exception(() => sut.RunClient(session, clientParameters));

		// Assert
		Assert.NotNull(exception);
		Assert.IsType<ArgumentException>(exception);
		Assert.Equal("No client path provided. Cannot start a client session.", exception.Message);
	}

	[Theory]
	[AutoMoqData]
	public void RunClient_StartsClient(
		[Frozen] Mock<IProcessProvider> processProviderMock,
		Session session,
		ClientParameters clientParameters,
		ClientSession sut)
	{
		// Arrange
		var expectedArguments = $"+connect {session.Ip}:{session.Port}";

		// Act
		sut.RunClient(session, clientParameters);

		// Assert
		processProviderMock.Verify(
			x => x.Start(It.Is<ProcessStartInfo>(x =>
				x.FileName == $"{clientParameters.Path} {expectedArguments}"
				&& x.UseShellExecute == true)),
			Times.Once());
	}
}