namespace BuddySave;

public class ClientNotifier : IClientNotifier
{
    public void Notify(string text)
    {
        Console.WriteLine(text);
    }
}