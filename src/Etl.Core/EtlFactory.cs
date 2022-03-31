using Etl.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core
{
    public interface IEtlFactory
    {
        (EtlDef definition, Etl instance) Get(string dataFilePath);

        (EtlDef definition, Etl instance) GetFrom(string configFilePath);
    }

    public class EtlFactory : IEtlFactory
    {
        private readonly Dictionary<string, (EtlDef definition, Etl executor)> _caches = new();
        private readonly ConfigFilesSetting _setting;
        private readonly List<(Regex matcher, string configFile)> _matchers = new();
        private readonly XmlAttributeOverrides _loaderDefsAttrOverrides;

        public EtlFactory(EtlSetting setting, List<Type> loaderDefs)
        {
            _setting = setting?.ConfigFiles ?? new ConfigFilesSetting();

            foreach (var e in _setting.Matches)
                _matchers.Add((new Regex(e.Key, RegexOptions.Compiled), e.Value));

            _loaderDefsAttrOverrides = GetAttributeOverrides(loaderDefs);
        }

        public void Save(EtlDef config, string filePath)
        {
            var serializer = new XmlSerializer(typeof(EtlDef), _loaderDefsAttrOverrides);
            using var stream = new StreamWriter(filePath);

            serializer.Serialize(stream, config);
        }

        public (EtlDef definition, Etl instance) Get(string dataFilePath)
        {
            dataFilePath = dataFilePath.ToLower();
            foreach (var (matcher, configFile) in _matchers)
            {
                if (matcher.IsMatch(dataFilePath))
                    return GetFrom(Path.Combine(_setting.Folder, configFile));
            }

            var fileInfo = new FileInfo(dataFilePath);
            dataFilePath = $"{fileInfo.Directory}/{Path.GetFileNameWithoutExtension(fileInfo.Name)}.xml";

            return GetFrom(dataFilePath);
        }

        public (EtlDef definition, Etl instance) GetFrom(string configFilePath)
        {
            if (!_caches.TryGetValue(configFilePath, out var cache))
                lock (_caches)
                {
                    if (!_caches.TryGetValue(configFilePath, out cache))
                    {
                        var serializer = new XmlSerializer(typeof(EtlDef), _loaderDefsAttrOverrides);
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
                        var result = (def, new Etl(def));
                        _caches[configFilePath] = result;

                        return result;
                    }
                }

            return cache;
        }
        private static XmlAttributeOverrides GetAttributeOverrides(List<Type> loaderDefs)
        {
            var attrOverrides = new XmlAttributeOverrides();

            var attrs = new XmlAttributes();
            foreach (var e in loaderDefs)
                attrs.XmlArrayItems.Add(new XmlArrayItemAttribute
                {
                    ElementName = e.Name.Replace("LoaderDef", ""),
                    Type = e
                });
            attrOverrides.Add(typeof(EtlDef), nameof(EtlDef.Loaders), attrs);

            return attrOverrides;
        }
    }
}
