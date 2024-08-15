using Microsoft.Extensions.Logging;

namespace BuddySave.Notifications;

public class ClientNotifier(ILogger<ClientNotifier> logger) : IClientNotifier
{
    public void Notify(string text)
    {
        logger.LogInformation(text);
    }
}