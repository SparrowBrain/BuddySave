using BuddySave.Configuration;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using Newtonsoft.Json;

namespace BuddySave
{
    internal class Program
    {
        private static readonly ISharedSaveOrchestrator SharedSaveOrchestrator;

        static Program()
        {
            var backupDirectory = new BackupDirectoryProvider();
            var saveCopier = new SaveCopier();
            var cloudManager = new CloudManager(backupDirectory, saveCopier);
            var clientNotifier = new ClientNotifier();
            SharedSaveOrchestrator = new SharedSaveOrchestrator(cloudManager, clientNotifier);
        }

        private static async Task Main()
        {
            Console.WriteLine("∞∞∞∞∞∞∞ Buddy Save ∞∞∞∞∞∞∞");

            var config = await LoadConfiguration();
            var gameSave = new GameSave(config.Game.SaveName, config.Game.SavePath, Path.Combine(config.CloudPath, config.Game.Name));
            await Run(gameSave);

            Console.WriteLine("Bye Buddy! ;)");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
            var configFile = await File.ReadAllTextAsync("config.json");
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