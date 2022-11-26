using Newtonsoft.Json;
using NLog;

namespace BuddySave.Configuration;

public class ConfigurationLoader : IConfigurationLoader
{
    private readonly ILogger _logger;

    public ConfigurationLoader(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<IBuddySaveConfiguration> Load()
    {
        _logger.Info("Reading configuration file...");
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