using NLog;

namespace BuddySave.Notifications;

public class ClientNotifier : IClientNotifier
{
    private readonly ILogger _logger;

    public ClientNotifier(ILogger logger)
    {
        _logger = logger;
    }

    public void Notify(string text)
    {
        _logger.Info(text);
    }
}