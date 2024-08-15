using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuddySave.Configuration;

public class ConfigurationLoader(ILogger<ConfigurationLoader> logger) : IConfigurationLoader
{
    public async Task<IBuddySaveConfiguration> Load()
    {
        logger.LogInformation("Reading configuration file...");
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

        config.GameSave.CloudPath = Path.Combine(config.CloudPath, config.GameSave.GameName);
        
        return config;
    }
}