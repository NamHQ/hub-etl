using Etl.Core;
using Etl.Storage;
using Etl.Tranformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Etl.ConsoleApp
{
    partial class Program
    {
        static void Main(string[] arguments)
        {
            var args = BuildArgs(arguments);
            if (string.IsNullOrEmpty(args.DataFile))
                return;

            var start = DateTime.Now;
            Console.WriteLine($"\n============================ START {start} ================================");

            try
            {
                var configuration = BuildConfiguration();
                var services = new ServiceCollection();
                services.AddSingleton(configuration);
                services.AddEtlTransformation(configuration);
                services.AddEtl(configuration,
                    new List<Assembly> { typeof(Tranformation.Setup).Assembly },
                    new List<Assembly> { typeof(CsvLoader).Assembly });

                var sp = services.BuildServiceProvider();


                //sp.GetRequiredService<EtlFactory>().Save(args.Config, "../../../../../Data/Delimiter-demo.xml");
                //sp.GetRequiredService<EtlFactory>().Save(args.Config, "../../../../../Data/FDC_CRVD3071_CD028_2111161907.xml");
                Console.Write($"Initializing... ");
                
                var builder = sp.GetRequiredService<WorkflowBuilder>()
                   .SetConfig(args.Config)
                   .SetConfig(args.ConfigFile)                  //Override args.Config
                                                                //.AddLoaders(new ConsoleLoader())
                   .Subcribe(events => events.ConsoleLog(
                        onScanned: args.OnScanned,
                        onExtracting: args.OnExtracting,
                        onExtracted: args.OnExtracted,
                        onTransformed: args.OnTransformed,
                        onTransformedBatch: args.OnTransformedBatch,
                        OnStatusIntervalSeconds: args.OnStatusIntervalSeconds));

                var workflow = builder.Build(args.DataFile);

                Console.WriteLine($"========{DateTime.Now.Subtract(start)}");

                workflow.Start(args.Take, args.Skip);
            }
            catch (Exception ex)
            {
                do
                {
                    Console.Error.WriteLine(ex);
                    ex = ex.InnerException;
                }
                while (ex != null);
            }

            Console.WriteLine($"\n============================ END {DateTime.Now.Subtract(start)} ================================");

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

        static AppArgument BuildArgs(params string[] arguments)
        {
            AppArgument args = new();
            var dataFoler = "../../../../../Data";

            //args.Config = ConfigTest.CreateCD028();
            args.Config = ConfigTest.CreateDelimiterDemoConfig();
            //EtlFactory.Save(args.Config, $"{dataFoler}/Delimiter-demo.xml");
            arguments = new string[] {
                //$"D:/Data/Parser/FDC_CRVD3071_CD028_2111161907",
                //$"-config={dataFoler}/FDC_CRVD3071_CD028_2111161907.xml",

                $"{dataFoler}/Delimiter-demo",
                $"-config={dataFoler}/Delimiter-demo.xml",

                //"-skip=1",
                //"-take=1",
                //"-onScanned",
                //"-onExtracting",
                //"-onExtracted",
                //"-onTransformed",
                //"-onTransformedBatch",
                "-onStatusIntervalSeconds=1"
            };

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
                        || SetConfig(e, "-take", val => args.Take = int.Parse(val))
                        || SetConfig(e, "-skip", val => args.Skip = int.Parse(val))
                        || SetConfig(e, "-onScanned", _ => args.OnScanned = true)
                        || SetConfig(e, "-onExtracting", _ => args.OnExtracting = true)
                        || SetConfig(e, "-onExtracted", _ => args.OnExtracted = true)
                        || SetConfig(e, "-onTransformed", _ => args.OnTransformed = true)
                        || SetConfig(e, "-onTransformedBatch", _ => args.OnTransformedBatch = true)
                        || SetConfig(e, "-onStatusIntervalSeconds", val => args.OnStatusIntervalSeconds= int.Parse(val));
                }

            return args;
        }

        private static void PrintSyntax()
        {
            Console.WriteLine(" Syntax: etl dataFile [-Options]");
            Console.WriteLine(" Options:");
            Console.WriteLine("     -config=(configFile.xml),");
            Console.WriteLine("     -take=(number)");
            Console.WriteLine("     -skip=(number)");
            Console.WriteLine("     -onScanned");
            Console.WriteLine("     -onExtracting");
            Console.WriteLine("     -onExtracted");
            Console.WriteLine("     -onTransformed");
            Console.WriteLine("     -onTransformedBatch");
            Console.WriteLine("     -onStatusIntervalSeconds");
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
