namespace BuddySave.Configuration;

public interface IConfigurationLoader
{
    Task<IBuddySaveConfiguration> Load();
}