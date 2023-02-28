using System.Diagnostics;
using BuddySave.Core.Models;
using BuddySave.System;
using NLog;

namespace BuddySave.Core;

public class GamingSession : IGamingSession
{
    private readonly ILogger _logger;
    private readonly ISharedSaveOrchestrator _sharedSaveOrchestrator;
    private readonly IProcessProvider _processProvider;

    public GamingSession(ILogger logger, ISharedSaveOrchestrator sharedSaveOrchestrator, IProcessProvider processProvider)
    {
        _logger = logger;
        _sharedSaveOrchestrator = sharedSaveOrchestrator;
        _processProvider = processProvider;
    }

    public async Task Run(GameSave gameSave, Session session, ServerParameters serverParameters)
    {
        if (string.IsNullOrWhiteSpace(serverParameters.Path))
        {
            throw new ArgumentException("No server path provided. Cannot start a gaming session.");
        }

        await _sharedSaveOrchestrator.Load(gameSave, session);
        await RunServer(serverParameters);
        await _sharedSaveOrchestrator.Save(gameSave, session);
    }

    private async Task RunServer(ServerParameters serverParameters)
    {
        var workingDirectory = Path.GetDirectoryName(serverParameters.Path);
        var startInfo = new ProcessStartInfo()
        {
            FileName = serverParameters.Path,
            Arguments = serverParameters.Arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };

        var process = _processProvider.Start(startInfo);
        _logger.Info(@$"Server started, waiting for exit: ""{serverParameters}""");

        await _processProvider.WaitForExitAsync(process);
        _logger.Info("Server exited");
    }
}