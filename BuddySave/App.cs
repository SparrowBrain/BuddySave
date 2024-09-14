using BuddySave.Configuration;
using BuddySave.Core;
using BuddySave.Core.Models;
using Microsoft.Extensions.Logging;

namespace BuddySave;

internal sealed class App(
    ISharedSaveOrchestrator sharedSaveOrchestrator,
    IServerSession serverSession,
    ILogger<App> logger,
    IConfigurationLoader configurationLoader)
{
    public async Task Start()
    {
        logger.LogInformation("Start");
        Console.WriteLine("∞∞∞∞∞∞∞ Buddy Save ∞∞∞∞∞∞∞");

        try
        {
            var configuration = await configurationLoader.Load();
            await Run(configuration.GameSave, configuration.Session, configuration.ServerParameters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start main App.");
        }

        Console.WriteLine("Bye Buddy! ;)");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        Console.WriteLine();
        logger.LogInformation("Exit");
    }
    
    private async Task Run(GameSave gameSave, Session session, ServerParameters serverParameters)
    {
        var input = string.Empty;
        while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Waiting for action command (run, load, save, exit):");
            input = Console.ReadLine();

            switch (input?.ToLowerInvariant())
            {
                case "run":
                    await serverSession.RunServerWithAutoSave(gameSave, session, serverParameters);
                    break;

                case "load":
                    await sharedSaveOrchestrator.Load(gameSave, session);
                    break;

                case "save":
                    await sharedSaveOrchestrator.Save(gameSave, session);
                    break;

                case "exit":
                    break;
            }
        }
    }
}