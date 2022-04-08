using Etl.Core.Settings;
using Etl.Core.Transformation;
using Etl.Core.Transformation.Fields;
using Etl.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Etl.Core
{
    public interface IEtlFactory
    {
        (Etl definition, EtlInst instance) Get(string dataFilePath);

        (Etl definition, EtlInst instance) GetFrom(string configFilePath);
    }

    public class EtlFactory : IEtlFactory
    {
        private readonly Dictionary<string, (Etl definition, EtlInst instance)> _caches = new();
        private readonly ConfigFilesSetting _setting;
        private readonly List<(Regex matcher, string configFile)> _matchers = new();
        private readonly XmlAttributeOverrides _attributeOverrides = new();

        public EtlFactory(EtlSetting setting, List<Type> fieldDefs, List<Type> actionDefs, List<Type> loaderDefs)
        {
            _setting = setting?.ConfigFiles ?? new ConfigFilesSetting();

            foreach (var e in _setting.Matches)
                _matchers.Add((new Regex(e.Key, RegexOptions.Compiled), e.Value));

            AddFieldDefsOverrides(_attributeOverrides, fieldDefs);
            AddActionDefsOverrides(_attributeOverrides, actionDefs);
            AddLoaderDefsOverrides(_attributeOverrides, loaderDefs);
        }

        public void Save(Etl config, string filePath)
        {
            var serializer = new XmlSerializer(typeof(Etl), _attributeOverrides);
            filePath = FilePath.GetFullPath(filePath);
            using var stream = new StreamWriter(filePath);

            serializer.Serialize(stream, config);
        }

        public (Etl definition, EtlInst instance) Get(string dataFilePath)
        {
            dataFilePath = dataFilePath.ToLower();
            foreach (var (matcher, configFile) in _matchers)
            {
                if (matcher.IsMatch(dataFilePath))
                    return GetFrom(Path.Combine(_setting.Folder, configFile));
            }

            dataFilePath = FilePath.GetFullPath(dataFilePath);
            var fileInfo = new FileInfo(dataFilePath);
            dataFilePath = $"{fileInfo.Directory}/{Path.GetFileNameWithoutExtension(fileInfo.Name)}.xml";

            return GetFrom(dataFilePath);
        }

        public (Etl definition, EtlInst instance) GetFrom(string configFilePath)
        {
            if (!_caches.TryGetValue(configFilePath, out var cache))
                lock (_caches)
                {
                    if (!_caches.TryGetValue(configFilePath, out cache))
                    {
                        var serializer = new XmlSerializer(typeof(Etl), _attributeOverrides);
                        //serializer.UnknownNode += (sender, e) => Console.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
                        //serializer.UnknownAttribute += (sender, e) => Console.WriteLine("Unknown Attribute " + e.Attr.Name + "='" + e.Attr.Value + "'");
                        //serializer.UnknownElement += (sender, e) => Console.WriteLine("Unknown Element:" + e.Element, e);
                        //serializer.UnreferencedObject += (sender, e) => Console.WriteLine("Unknown UnreferencedObject " + e.ToString());

                        if (!File.Exists(configFilePath))
                            throw new Exception($"Not found config file '{configFilePath}'");

                        using var stream = new FileStream(configFilePath, FileMode.Open);
                        var def = (Etl)serializer.Deserialize(stream);
                        var result = (def, new EtlInst(def));
                        _caches[configFilePath] = result;

                        return result;
                    }
                }

            return cache;
        }
        private static void AddLoaderDefsOverrides(XmlAttributeOverrides attrOverrides, List<Type> loaderDefs)
        {
            var attrs = new XmlAttributes();
            foreach (var e in loaderDefs)
                attrs.XmlArrayItems.Add(new XmlArrayItemAttribute
                {
                    ElementName = e.Name.Replace("Loader", ""),
                    Type = e
                });
            attrOverrides.Add(typeof(Etl), nameof(Etl.Loaders), attrs);
        }

        private static void AddFieldDefsOverrides(XmlAttributeOverrides attrOverrides, List<Type> fieldDefs)
        {
            var attrs = new XmlAttributes();
            foreach (var e in fieldDefs)
                attrs.XmlArrayItems.Add(new XmlArrayItemAttribute
                {
                    ElementName = e.Name.Replace("Field", ""),
                    Type = e
                });
            attrOverrides.Add(typeof(Transformer), nameof(Transformer.Fields), attrs);
            attrOverrides.Add(typeof(RecordField), nameof(RecordField.Fields), attrs);
        }

        private static void AddActionDefsOverrides(XmlAttributeOverrides attrOverrides, List<Type> actionDefs)
        {
            var attrs = new XmlAttributes();
            foreach (var e in actionDefs)
                attrs.XmlArrayItems.Add(new XmlArrayItemAttribute
                {
                    ElementName = e.Name.Replace("Action", ""),
                    Type = e
                });
            attrOverrides.Add(typeof(PipeLineField), nameof(PipeLineField.Actions), attrs);
        }
    }
}
