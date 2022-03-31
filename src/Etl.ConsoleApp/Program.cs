using Etl.Core;
using Etl.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etl.ConsoleApp
{
    partial class Program
    {
        static void Main(string[] arguments)
        {
            Console.WriteLine($"\n============================ START {DateTime.Now} ================================");

            try
            {
                var configuration = BuildConfiguration();
                var args = BuildArgs(configuration, arguments);

                var services = new ServiceCollection();
                services.AddSingleton(configuration);
                services.AddEtl(configuration, typeof(CsvLoader).Assembly);

                var sp = services.BuildServiceProvider();

                sp.GetRequiredService<EtlFactory>().Save(args.Config, "../../../../../Data/Delimiter-demo.xml");

                sp.GetRequiredService<Workflow>()
                   .SetConfig(args.Config)
                   .SetConfig(args.ConfigFile)                  //Override args.Config
                   //.AddLoaders(new CsvLoaderDef())
                   //.AddLoaders(new ConsoleLoader())
                   .Subcribe(events => events.ConsoleLog(
                        onScanned: args.OnScanned,
                        onExtracting: args.OnExtracting,
                        onExtracted: args.OnExtracted,
                        onTransformed: args.OnTransformed,
                        onTransformedBatch: args.OnTransformedBatch))
                   .Start(args.DataFile, args.Take, args.Skip);
            }
            catch (Exception ex)
            {
                do
                {
                    Console.Error.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                while (ex != null);
            }

            Console.WriteLine($"\n============================ END {DateTime.Now} ================================");

#if DEBUG
            Console.ReadLine();
#endif
        }

        static IConfiguration BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json");

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(env))
                configBuilder.AddJsonFile("appsettings.{env}.json");

            return configBuilder.Build();
        }

        static AppArgument BuildArgs(IConfiguration appSetting, params string[] arguments)
        {
            AppArgument args = new();
            var dataFoler = "../../../../../Data";

            //args.Config = ConfigTest.CreateCD028();
            args.Config = ConfigTest.CreateDelimiterDemoConfig();
            //EtlFactory.Save(args.Config, $"{dataFoler}/Delimiter-demo.xml");
            arguments = new string[] {
                //$"{dataFoler}/FDC_CRVD3071_CD028_2111161907",
                //"-config={dataFoler}/FDC_CRVD3071_CD028_2111161907.xml",

                $"{dataFoler}/Delimiter-demo",
                //$"-config={dataFoler}/Delimiter-demo.xml",

                //"-hash=D:/DLL/HashSaltKeys/salt.xml",
                //"-cryptor=D:/DLL/keys.xml",
                //"-skip=1",
                //"-take=1",
                //"-onScanned",
                //"-onExtracting",
                //"-onExtracted",
                //"-onTransformed",
                "-onTransformedBatch"
            };

            string hashFilePath = string.Empty;
            string cryptorFilePath = string.Empty;

            if (arguments.Length == 0)
                PrintSyntax();
            else
                foreach (var e in arguments)
                {
                    if (args.DataFile == null)
                    {
                        args.DataFile = e;
                        continue;
                    }

                    var _ =
                        SetConfig(e, "-config", val => args.ConfigFile = val)
                        || SetConfig(e, "-hash", val => hashFilePath = val)
                        || SetConfig(e, "-cryptor", val => cryptorFilePath = val)
                        || SetConfig(e, "-take", val => args.Take = int.Parse(val))
                        || SetConfig(e, "-skip", val => args.Skip = int.Parse(val))
                        || SetConfig(e, "-onScanned", _ => args.OnScanned = true)
                        || SetConfig(e, "-onExtracting", _ => args.OnExtracting = true)
                        || SetConfig(e, "-onExtracted", _ => args.OnExtracted = true)
                        || SetConfig(e, "-onTransformed", _ => args.OnTransformed = true)
                        || SetConfig(e, "-onTransformedBatch", _ => args.OnTransformedBatch = true);
                }

            return args;
        }

        private static void PrintSyntax()
        {
            Console.WriteLine(" Syntax: MPParser dataFile [-Options]");
            Console.WriteLine(" Options:");
            Console.WriteLine("     -config=(configFile.xml),");
            Console.WriteLine("     -hash=(salt.xml),");
            Console.WriteLine("     -cryptor=(key.xml),");
            Console.WriteLine("     -take=(number)");
            Console.WriteLine("     -skip=(number)");
            Console.WriteLine("     -onScanned");
            Console.WriteLine("     -onExtracting");
            Console.WriteLine("     -onExtracted");
            Console.WriteLine("     -onTransformed");
            Console.WriteLine("     -onTransformedBatch");
        }

        private static bool SetConfig(string configValue, string key, Action<string> setValue)
        {
            int i = configValue.IndexOf('=');
            var cmd = i < 0 ? configValue : configValue[..i];
            var val = i < 0 ? string.Empty : configValue[(i + 1)..];

            if (key.Equals(cmd, StringComparison.OrdinalIgnoreCase))
            {
                setValue?.Invoke(val);
                return true;
            }
            return false;
        }

    }
}
