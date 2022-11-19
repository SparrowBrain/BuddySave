using Newtonsoft.Json;

namespace BuddySave
{
    internal class Program
    {
        private static readonly ISharedSaveOrchestrator SharedSaveOrchestrator;
        
        static Program()
        {
            SharedSaveOrchestrator = new SharedSaveOrchestrator(new CloudManager(), new ClientNotifier());
        }
        
        private static async Task Main()
        {
            Console.WriteLine("∞∞∞∞∞∞∞ Buddy Save ∞∞∞∞∞∞∞");
            
            var config = await LoadConfiguration();
            var gameSave = new GameSave(config.Name, config.LocalPath, config.CloudPath);
            await Run(gameSave);
            
            Console.WriteLine("Bye Buddy! ;)");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task Run(GameSave gameSave)
        {
            Console.WriteLine("Input desired action (start, stop, exit):");
            var input = string.Empty;
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                input = Console.ReadLine();
                switch (input)
                {
                    case "start":
                        await SharedSaveOrchestrator.Load(gameSave);
                        break;
                    case "stop":
                        SharedSaveOrchestrator.Save(gameSave);
                        break;
                    case "exit":
                        break;
                }
            }
        }

        private static async Task<Configuration> LoadConfiguration()
        {
            Console.WriteLine("Reading configuration file...");
            var configFile = await File.ReadAllTextAsync("config.json");
            if (string.IsNullOrWhiteSpace(configFile))
            {
                throw new FileNotFoundException("Configuration file was not found", "config.json");
            }

            var config = JsonConvert.DeserializeObject<Configuration>(configFile);
            if (config == null)
            {
                throw new FileLoadException("Could not load configuration", "config.json");
            }
            
            return config;
        }
    }
}