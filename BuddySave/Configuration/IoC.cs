using BuddySave.Core;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using BuddySave.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace BuddySave.Configuration;

public static class IoC
{
    public static ServiceProvider Setup()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        
        var serviceProvider = new ServiceCollection()
            .AddTransient<App>()
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddNLog(config);
            })
            .AddSingleton<IConfigurationLoader, ConfigurationLoader>()
            .AddSingleton<ISaveCopier, SaveCopier>()
            .AddSingleton<IBackupDirectoryProvider, BackupDirectoryProvider>()
            .AddSingleton<IRollingBackups, RollingBackups>()
            .AddSingleton<IBackupManager, BackupManager>()
            .AddSingleton<IGameSaveSyncManager, GameSaveSyncManager>()
            .AddSingleton<IClientNotifier, ClientNotifier>()
            .AddSingleton<ILockManager, LockManager>()
            .AddSingleton<IProcessProvider, ProcessProvider>()
            .AddSingleton<ISharedSaveOrchestrator, SharedSaveOrchestrator>()
            .AddSingleton<IGamingSession, ServerSession>()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IFileInfoProvider, FileInfoProvider>()
            .AddSingleton<ILatestSaveTypeProvider, LatestSaveTypeProvider>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}