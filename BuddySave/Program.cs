﻿using System.Diagnostics;
using BuddySave.Configuration;
using BuddySave.Core;
using BuddySave.Core.Models;
using BuddySave.FileManagement;
using BuddySave.Notifications;
using BuddySave.System;
using NLog;

namespace BuddySave
{
    internal class Program
    {
        private static readonly ISharedSaveOrchestrator SharedSaveOrchestrator;
        private static readonly Logger Logger;
        private static readonly IConfigurationLoader ConfigurationLoader;

        static Program()
        {
            Logger = LogManager.GetLogger("BuddySave");
            ConfigurationLoader = new ConfigurationLoader(Logger);
            var saveCopier = new SaveCopier();
            var backupDirectoryProvider = new BackupDirectoryProvider(new DateTimeProvider());
            var rollingBackups = new RollingBackups(Logger, backupDirectoryProvider, saveCopier);
            var backupManager = new BackupManager(rollingBackups, saveCopier, Logger);
            var gameSaveSyncManager = new GameSaveSyncManager(Logger, saveCopier, backupManager);
            var clientNotifier = new ClientNotifier(Logger);
            var lockManager = new LockManager();
            SharedSaveOrchestrator = new SharedSaveOrchestrator(Logger, gameSaveSyncManager, lockManager, clientNotifier);
        }

        private static async Task Main()
        {
            Logger.Info("Start");
            Console.WriteLine("∞∞∞∞∞∞∞ Buddy Save ∞∞∞∞∞∞∞");

            try
            {
                var configuration = await ConfigurationLoader.Load();
                await Run(configuration.GameSave, configuration.Session, configuration.ServerPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Console.WriteLine("Bye Buddy! ;)");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Logger.Info("Exit");
        }

        private static async Task Run(GameSave gameSave, Session session, string serverPath)
        {
            var input = string.Empty;
            while (!string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Waiting for action command (run, load, save, exit):");
                input = Console.ReadLine();

                switch (input?.ToLowerInvariant())
                {
                    case "run":
                        if (string.IsNullOrWhiteSpace(serverPath))
                        {
                            throw new Exception("No server path provided. Cannot start a gaming session.");
                        }

                        await SharedSaveOrchestrator.Load(gameSave, session);
                        await RunServer(serverPath);
                        await SharedSaveOrchestrator.Save(gameSave, session);
                        break;

                    case "load":
                        await SharedSaveOrchestrator.Load(gameSave, session);
                        break;

                    case "save":
                        await SharedSaveOrchestrator.Save(gameSave, session);

                        break;

                    case "exit":
                        break;
                }
            }
        }

        private static async Task RunServer(string serverPath)
        {
            var workingDirectory = Path.GetDirectoryName(serverPath);
            var startInfo = new ProcessStartInfo()
            {
                FileName = serverPath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);
            Logger.Info(@$"Server started, waiting for exit: ""{serverPath}""");
            await process.WaitForExitAsync();
            Logger.Info("Server exited");
        }
    }
}