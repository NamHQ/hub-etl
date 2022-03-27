using Etl.Core.Load;
using Etl.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core
{
    public interface IEtlFactory
    {
        (EtlDef definition, EtlExecutor executor) Load(string dataFilePath);

        (EtlDef definition, EtlExecutor executor) DirectlyLoad(string configFilePath);
    }

    public class EtlFactory : IEtlFactory
    {
        private readonly Dictionary<string, (EtlDef definition, EtlExecutor executor)> _caches = new();
        private readonly ConfigFilesSetting _setting;
        private readonly List<(Regex matcher, string configFile)> _matchers = new();
        private readonly Lazy<XmlAttributeOverrides> _lazyAttrOverrides;

        public EtlFactory(EtlSetting setting)
        {
            _setting = setting?.ConfigFiles ?? new ConfigFilesSetting();

            foreach (var e in _setting.Matches)
                _matchers.Add((new Regex(e.Key, RegexOptions.Compiled), e.Value));

            _lazyAttrOverrides = new Lazy<XmlAttributeOverrides>(() => GetAttributeOverrides(setting.References));
        }

        public void Save(EtlDef config, string fileName)
        {
            var serializer = new XmlSerializer(typeof(EtlDef), _lazyAttrOverrides.Value);
            var path = Path.Combine(_setting.Folder, fileName);
            using var stream = new StreamWriter(path);

            serializer.Serialize(stream, config);
        }

        public (EtlDef definition, EtlExecutor executor) Load(string dataFilePath)
        {
            dataFilePath = dataFilePath.ToLower();
            foreach (var (matcher, configFile) in _matchers)
            {
                if (matcher.IsMatch(dataFilePath))
                    return DirectlyLoad(Path.Combine(_setting.Folder, configFile));
            }

            var fileInfo = new FileInfo(dataFilePath);
            dataFilePath = $"{fileInfo.Directory}/{Path.GetFileNameWithoutExtension(fileInfo.Name)}.xml";

            return DirectlyLoad(dataFilePath);
        }

        public (EtlDef definition, EtlExecutor executor) DirectlyLoad(string configFilePath)
        {
            if (!_caches.TryGetValue(configFilePath, out var cache))
                lock (_caches)
                {
                    if (!_caches.TryGetValue(configFilePath, out cache))
                    {

                        var serializer = new XmlSerializer(typeof(EtlDef), _lazyAttrOverrides.Value);
                        serializer.UnknownNode += (sender, e) =>
                            Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);

                        serializer.UnknownAttribute += (sender, e) =>
                            Console.WriteLine("Unknown Attribute " + e.Attr.Name + "='" + e.Attr.Value + "'");

                        serializer.UnknownElement += (sender, e) =>
                            Console.WriteLine("Unknown Element:" + e.Element, e);

                        serializer.UnreferencedObject += (sender, e) =>
                            Console.WriteLine("Unknown UnreferencedObject " + e.ToString());

                        if (!File.Exists(configFilePath))
                            throw new Exception($"Not found config file '{configFilePath}'");

                        using var stream = new FileStream(configFilePath, FileMode.Open);
                        var def = (EtlDef)serializer.Deserialize(stream);
                        var result = (def, new EtlExecutor(def));
                        _caches[configFilePath] = result;

                        return result;
                    }
                }

            return cache;
        }
        private static XmlAttributeOverrides GetAttributeOverrides(List<string> references)
        {
            var attrOverrides = new XmlAttributeOverrides();
            var types = new List<Type> { typeof(ConsoleLoader) };

            if (references != null && references.Count > 0)
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
            attrOverrides.Add(typeof(EtlDef), nameof(EtlDef.Loaders), attrs);

            return attrOverrides;
        }
    }
}
