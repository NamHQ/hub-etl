using Etl.Core;
using Etl.Core.Transformation;
using Etl.Core.Utils;
using Etl.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Etl.ConsoleApp
{
    partial class Program
    {
        static void Main(string[] arguments)
        {
            try
            {
                var appSetting = BuildAppSetting();
                var args = BuildArgs(appSetting, arguments);
                if (args.DataFile == null)
                    return;

                var dataFileNoExtension = $"{args.DataFile.Directory}/{Path.GetFileNameWithoutExtension(args.DataFile.Name)}";

                var start = DateTime.Now;
                Console.WriteLine($"\n============================ START {start} ================================");

                new Workflow(appSetting, args.Config, args.DataFile.FullName)
                     .Subcribe(events => events.ConsoleLog(
                        onScanned: args.OnScanned,
                        onExtracting: args.OnExtracting,
                        onExtracted: args.OnExtracted,
                        onTransformed: args.OnTransformed,
                        onTransformedBatch: args.OnTransformedBatch)
                        )
                    .AddLoaders(new CsvLoader())
                    .Start(args.Context, args.Take, args.Skip);
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

        static IConfiguration BuildAppSetting()
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
            //EtlDefManager.Save(args.Config, $"{dataFoler}/Data/Delimiter-demo.xml", appSetting);
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
            else
                foreach (var e in arguments)
                {
                    if (args.DataFile == null)
                    {
                        args.DataFile = new FileInfo(e);
                        if (!args.DataFile.Exists)
                            throw new Exception($"Not existed data file {e}");
                        continue;
                    }

                    var _ = 
                        SetConfig(e, "-config", val =>
                            {
                                if (!File.Exists(val))
                                    throw new Exception($"Not existed config file {val}");
                                args.Config = EtlDefManager.Load(val, appSetting);
                            })
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

            if (args.Config == null && args.DataFile != null)
            {
                var filePath = $"{args.DataFile.Directory}/{Path.GetFileNameWithoutExtension(args.DataFile.Name)}.xml";
                if (!File.Exists(filePath))
                    throw new Exception($"Not existed config file {filePath}");

                args.Config = EtlDefManager.Load(filePath, appSetting);
            }

            args.Context = GetContext(hashFilePath, cryptorFilePath);

            return args;
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

        public static Context GetContext(string hashFilePath, string cryptorFilePath)
        {
            string saltString = null;
            if (!string.IsNullOrEmpty(hashFilePath))
            {
                if (!File.Exists(hashFilePath))
                    throw new Exception($"File not exist {hashFilePath}");

                var doc = new XmlDocument();
                doc.Load(hashFilePath);
                saltString = Cryptor.Decrypt(doc.SelectSingleNode("//salts/salt").Attributes["value"].Value);
            }

            byte[] encryptorKey = null;
            byte[] encryptorIv = null;
            if (!string.IsNullOrEmpty(cryptorFilePath))
            {
                if (!File.Exists(cryptorFilePath))
                    throw new Exception($"File not exist {cryptorFilePath}");
                var doc = new XmlDocument();
                doc.Load(cryptorFilePath);
                var element = doc.SelectNodes("//keys/key")[0];

                encryptorKey = Encoding.ASCII.GetBytes(Cryptor.Decrypt(element.Attributes["value"].Value));
                encryptorIv = Encoding.ASCII.GetBytes(Cryptor.Decrypt(element.Attributes["iv"].Value));
            }

            return new Context(string.Empty, encryptorKey, encryptorIv, saltString);
        }
    }
}
