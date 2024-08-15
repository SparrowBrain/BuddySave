using BuddySave.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuddySave;

internal static class Program
{
    private static async Task Main()
    {
        var serviceProvider = IoC.Setup();
        var app = serviceProvider.GetRequiredService<App>();
        await app.Start();
    }
}