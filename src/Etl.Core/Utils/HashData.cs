using System.Security.Cryptography;
using System.Text;

namespace Etl.Core.Utils
{
    class HashData
    {
        public static string SHA2_512(string text)
        {
            UTF8Encoding encoder = new ();
            SHA512Managed shaHasher = new ();
            byte[] bytes = shaHasher.ComputeHash(encoder.GetBytes(text));
            return ByteArrayToString(bytes);
        }

        public static string SHA2_256(string text)
        {
            UTF8Encoding encoder = new();
            SHA256Managed shaHasher = new();
            byte[] hashedDataBytes = shaHasher.ComputeHash(encoder.GetBytes(text));
            return ByteArrayToString(hashedDataBytes);
        }

        static string ByteArrayToString(byte[] bytes)
        {
            StringBuilder sb = new(string.Empty);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("X2"));

            return sb.ToString();
        }
    }
}
