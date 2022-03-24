using Etl.Core.Load;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Etl.Core
{
    public static class EtlDefManager
    {
        private static readonly Dictionary<string, EtlDef> _caches = new();

        private static XmlAttributeOverrides _attrOverrides;

        public static void Save(EtlDef config, string filePath, IConfiguration appSetting)
        {
            lock (_caches)
            {
                _caches[filePath] = config;
            }

            var serializer = new XmlSerializer(typeof(EtlDef), GetAttributeOverrides(appSetting));
            using var stream = new StreamWriter(filePath);

            serializer.Serialize(stream, config);
        }

        public static EtlDef Load(string filePath, IConfiguration appSetting)
        {
            if (!_caches.ContainsKey(filePath))
                lock (_caches)
                {
                    if (!_caches.ContainsKey(filePath))
                    {
                        var serializer = new XmlSerializer(typeof(EtlDef), GetAttributeOverrides(appSetting));
                        //serializer.UnknownNode += (sender, e) =>
                        //    Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);

                        //serializer.UnknownAttribute += (sender, e) =>
                        //    Console.WriteLine("Unknown Attribute " + e.Attr.Name + "='" + e.Attr.Value + "'");

                        serializer.UnknownElement += (sender, e) =>
                            Console.WriteLine("Unknown Element:" + e.Element, e);

                        serializer.UnreferencedObject += (sender, e) =>
                            Console.WriteLine("Unknown UnreferencedObject " + e.ToString());

                        using var stream = new FileStream(filePath, FileMode.Open);
                        _caches[filePath] = (EtlDef)serializer.Deserialize(stream);
                    }
                }

            return _caches[filePath];
        }

        private static XmlAttributeOverrides GetAttributeOverrides(IConfiguration appSetting)
        {
            if (_attrOverrides != null)
                return _attrOverrides;

            _attrOverrides = new XmlAttributeOverrides();
            var types = new List<Type> { typeof(ConsoleLoader) };

            var references = appSetting.GetSection("References").Get<string[]>();
            if (references != null)
                types.AddRange(references
                    .Select(e =>
                    {
                        var fileInfo = new FileInfo(e);
                        if (!fileInfo.Exists)
                            throw new Exception($"File not found {e} to refer.");
                        return Assembly.LoadFrom(fileInfo.FullName);
                    })
                    .SelectMany(e => e.GetTypes())
                    .Where(e => typeof(Loader).IsAssignableFrom(e) && !e.IsAbstract));

            var attrs = new XmlAttributes();
            foreach (var e in types)
                attrs.XmlArrayItems.Add(new XmlArrayItemAttribute
                {
                    ElementName = e.Name.Replace("Loader", ""),
                    Type = e
                });
            _attrOverrides.Add(typeof(EtlDef), nameof(EtlDef.Loaders), attrs);

            return _attrOverrides;
        }
    }
}
