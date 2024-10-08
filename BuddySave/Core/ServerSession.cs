﻿using System.Diagnostics;
using BuddySave.Core.Models;
using BuddySave.System;
using Microsoft.Extensions.Logging;

namespace BuddySave.Core;

public class ServerSession(
    ILogger<ServerSession> logger,
    ISharedSaveOrchestrator sharedSaveOrchestrator,
    IProcessProvider processProvider)
    : IServerSession
{
    public async Task RunServerWithAutoSave(GameSave gameSave, Session session, ServerParameters serverParameters)
    {
        if (string.IsNullOrWhiteSpace(serverParameters.Path))
        {
            throw new ArgumentException("No server path provided. Cannot start a gaming session.");
        }

        var loadResult = await sharedSaveOrchestrator.Load(gameSave, session);
        if (loadResult is not OrchestratorResult.Loaded)
        {
            return;
        }
        
        var process = StartServer(serverParameters);
        await WaitForServerToStop(process);
        await sharedSaveOrchestrator.Save(gameSave, session);
    }

    private Process StartServer(ServerParameters serverParameters)
    {
        var workingDirectory = Path.GetDirectoryName(serverParameters.Path);
        var startInfo = new ProcessStartInfo
        {
            FileName = serverParameters.Path,
            Arguments = serverParameters.Arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };

        var process = processProvider.Start(startInfo);
        var serverString = string.IsNullOrEmpty(serverParameters.Arguments)
            ? serverParameters.Path
            : $"{serverParameters.Path} {serverParameters.Arguments}";
        logger.LogInformation(@$"Server started, waiting for exit: ""{serverString}""");

        return process;
    }

    private async Task WaitForServerToStop(Process process)
    {
        await processProvider.WaitForExitAsync(process);
        logger.LogInformation("Server exited");
    }
}