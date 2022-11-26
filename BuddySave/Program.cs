using BuddySave.Configuration;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using NLog;

namespace BuddySave
{
    internal class Program
    {
        private static readonly ISharedSaveOrchestrator SharedSaveOrchestrator;
        private static readonly Logger Logger;
        private static readonly IConfigurationLoader ConfigurationLoader;

        static Program()
        {
            Logger = LogManager.GetLogger("BuddySave");
            ConfigurationLoader = new ConfigurationLoader(Logger);
            var saveCopier = new SaveCopier();
            var backupDirectoryProvider = new BackupDirectoryProvider();
            var backupManager = new BackupManager(backupDirectoryProvider, saveCopier, Logger);
            var gameSaveSyncManager = new GameSaveSyncManager(saveCopier, backupManager);
            var clientNotifier = new ClientNotifier();
            var lockManager = new LockManager();
            SharedSaveOrchestrator = new SharedSaveOrchestrator(gameSaveSyncManager, lockManager, clientNotifier);
        }

        private static async Task Main()
        {
            Logger.Info("Start");
            Console.WriteLine("∞∞∞∞∞∞∞ Buddy Save ∞∞∞∞∞∞∞");

            try
            {
                var configuration = await ConfigurationLoader.Load();
                await Run(configuration.GameSave, configuration.Session);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Console.WriteLine("Bye Buddy! ;)");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Logger.Info("Exit");
        }

        private static async Task Run(GameSave gameSave, Session session)
        {
            var input = string.Empty;
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Waiting for action command (load, save, exit):");
                input = Console.ReadLine();
                switch (input?.ToLowerInvariant())
                {
                    case "load":
                        await SharedSaveOrchestrator.Load(gameSave, session);
                        break;

                    case "save":
                        await SharedSaveOrchestrator.Save(gameSave, session);
                        break;

                    case "exit":
                        break;
                }
            }
        }
    }
}