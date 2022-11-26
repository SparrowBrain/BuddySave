using BuddySave.Configuration;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using Newtonsoft.Json;
using NLog;

namespace BuddySave
{
    internal class Program
    {
        private static readonly ISharedSaveOrchestrator SharedSaveOrchestrator;
        private static readonly Logger Logger;

        static Program()
        {
            Logger = LogManager.GetLogger("BuddySave");
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
                var config = await LoadConfiguration();
                var gameSave = new GameSave(config.Game.SaveName, config.Game.SavePath, Path.Combine(config.CloudPath, config.Game.Name));
                await Run(gameSave);
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

        private static async Task Run(GameSave gameSave)
        {
            var input = string.Empty;
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Waiting for action command (load, save, exit):");
                input = Console.ReadLine();
                switch (input?.ToLowerInvariant())
                {
                    case "load":
                        await SharedSaveOrchestrator.Load(gameSave);
                        break;

                    case "save":
                        SharedSaveOrchestrator.Save(gameSave);
                        break;

                    case "exit":
                        break;
                }
            }
        }

        private static async Task<BuddySaveConfiguration> LoadConfiguration()
        {
            Console.WriteLine("Reading configuration file...");
            var configFile = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new FileNotFoundException("Configuration file was not found", "config.json");
            }

            var config = JsonConvert.DeserializeObject<BuddySaveConfiguration>(configFile);
            if (config == null)
            {
                throw new FileLoadException("Could not load configuration", "config.json");
            }

            return config;
        }
    }
}