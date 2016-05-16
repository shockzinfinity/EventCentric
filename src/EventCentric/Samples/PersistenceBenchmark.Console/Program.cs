﻿using EventCentric.MicroserviceFactory;
using Microsoft.Practices.Unity;
using System;

namespace PersistenceBenchmark.ConsoleHost
{
    public class Program
    {
        public static bool VerboseIsEnabled = false;

        static void Main(string[] args)
        {
            var plugin = DbManager.SetPlugin(PersistencePlugin.SqlServer);

            PrintWelcomeMessage(plugin);

            DbManager.ResetDbs(plugin);
            var mainContainer = UnityConfig.GetConfiguredContainer(plugin);

            var user1App = UnityConfig.UserContainer1.Resolve<UserAppService>();
            var user2App = UnityConfig.UserContainer2.Resolve<UserAppService>();

            // Holding in memory messages
            // Expected: Events: waves * users * 2;

            Console.WriteLine("Press enter to start....");
            Console.ReadLine();


            // SQL SERVER ADO.NET --------------------------------------------
            // 100 througput,   completes in 35 s 10.000 messages    285 m/s
            if (plugin == PersistencePlugin.SqlServer)
            {
                user1App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
                user2App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
            }

            // IN-MEMORY-------------------------------------------------------
            // 100 througput,   completes in 0:20 s 17.204 messgaes    860 m/s
            if (plugin == PersistencePlugin.InMemory)
            {
                user1App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
                user2App.StressWithWavesOfConcurrentUsers(wavesCount: 5, concurrentUsers: 1000);
            }

            // SUPER TEST
            // 100 througput, completes in 07:08 s 40.000 messages     93 m/s
            //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 1000);
            //user2App.StressWithWavesOfConcurrentUsers(wavesCount: 10, concurrentUsers: 1000);

            //Light
            //user1App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);
            //user2App.StressWithWavesOfConcurrentUsers(wavesCount: 1, concurrentUsers: 1);

            Console.WriteLine("Press enter to stop and clean...");
            Console.ReadLine();
            if (plugin == PersistencePlugin.InMemory)
                UnityConfig.StatsMonitor.PrintStats();

            DbManager.DropDb(plugin);
        }

        private static void PrintWelcomeMessage(PersistencePlugin plugin)
        {
            string pluginSelectedName = "undefined";
            switch (plugin)
            {
                case PersistencePlugin.InMemory:
                    pluginSelectedName = "In-Memory";
                    break;
                case PersistencePlugin.SqlServer:
                    pluginSelectedName = "Sql Server";
                    break;
                case PersistencePlugin.SqlServerCe:
                    pluginSelectedName = "Sql Server Compact Edition";
                    break;
            }

            Console.WriteLine($"Welcome to persistence bench for {pluginSelectedName}");
        }
    }
}