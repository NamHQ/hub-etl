using System;
using System.Security.Cryptography;
using System.Text;

namespace Etl.Core.Utils
{
    public class Cryptor
    {
        private readonly static byte[] KEY = Encoding.ASCII.GetBytes("NGERNINHXEOUJMSUHGJIIXOOHYTTXBMP");
        private readonly static byte[] IV = Encoding.ASCII.GetBytes("afpgvmwledfptycr");

        public static string Decrypt(string text, byte[] key = null, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            try
            {
                using var cryptor = CreateCryptor(key, iv);
                using var decryptor = cryptor.CreateDecryptor();

                byte[] bytes = Convert.FromBase64String(text);
                text = Encoding.ASCII.GetString(decryptor.TransformFinalBlock(bytes, 0, bytes.Length));
                return text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Encrypt(string text, byte[] key = null, byte[] iv = null)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            try
            {
                using var cryptor = CreateCryptor(key, iv);
                using var encryptor = cryptor.CreateEncryptor();

                byte[] bytes = Encoding.ASCII.GetBytes(text);
                return Convert.ToBase64String(encryptor.TransformFinalBlock(bytes, 0, bytes.Length));

            }
            catch
            {
                return string.Empty;
            }
        }

        public static RijndaelManaged CreateCryptor(byte[] key, byte[] iv)
            => new()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                Key = key ?? KEY,
                IV = iv ?? IV,
            };
    }


}
