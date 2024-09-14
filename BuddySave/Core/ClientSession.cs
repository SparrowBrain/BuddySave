using System.Diagnostics;
using BuddySave.Core.Models;
using BuddySave.System;

namespace BuddySave.Core;

public class ClientSession(IProcessProvider processProvider) : IClientSession
{
	public void RunClient(Session session, ClientParameters clientParameters)
	{
		if (string.IsNullOrEmpty(clientParameters.Path))
		{
			throw new ArgumentException("No client path provided. Cannot start a client session.");
		}

		var arguments = $"+connect {session.Ip}:{session.Port}";
		var startInfo = new ProcessStartInfo
		{
			FileName = clientParameters.Path,
			Arguments = arguments,
		};

		processProvider.Start(startInfo);
	}
}