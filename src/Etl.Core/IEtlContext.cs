using Etl.Core.Settings;
using Etl.Core.Utils;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Etl.Core
{
    public interface IEtlContext
    {
        (byte[] key, byte[] iv) CryptorInfo { get; }

        string SaltHashString { get; }
    }

    public class EtlContext : IEtlContext
    {
        public (byte[] key, byte[] iv) CryptorInfo { get; }

        public string SaltHashString { get; }

        public EtlContext(EtlSetting etlSetting)
        {
            var hashFilePath = etlSetting?.Cryptor?.HashFile;
            var cryptorFilePath = etlSetting?.Cryptor?.CryptorFile;

            if (!string.IsNullOrEmpty(hashFilePath))
            {
                if (!File.Exists(hashFilePath))
                    throw new Exception($"Hash File not exist {hashFilePath}");

                var doc = new XmlDocument();
                doc.Load(hashFilePath);
                SaltHashString = Cryptor.Decrypt(doc.SelectSingleNode("//salts/salt").Attributes["value"].Value);
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

            CryptorInfo = (encryptorKey, encryptorIv);
        }
    }
}
