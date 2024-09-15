using BuddySave.Core.Models;
using BuddySave.System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuddySave.Core;

public class ClientSession(IProcessProvider processProvider, ILogger<ClientSession> logger) : IClientSession
{
	public void StartClient(Session session, ClientParameters clientParameters)
	{
		if (string.IsNullOrEmpty(clientParameters.Path))
		{
			throw new ArgumentException("No client path provided. Cannot start a client session.");
		}

		var arguments = clientParameters.Arguments.Replace("{{Ip}}", session.Ip).Replace("{{Port}}", session.Port);
		var startInfo = new ProcessStartInfo
		{
			FileName = $"{clientParameters.Path} {arguments}",
			UseShellExecute = true
		};

		processProvider.Start(startInfo);
		logger.LogInformation($@"Client started: ""{clientParameters.Path} {arguments}""");
	}
}