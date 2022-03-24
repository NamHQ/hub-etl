using System.Security.Cryptography;
using System.Text;

namespace Etl.Core.Transformation.Fields
{
    public class HashField : FieldBase<string>
    {
        protected override string Convert(string text, Context context)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return string.IsNullOrWhiteSpace(context?.SaltHashString) ? Hash(text) : Hash(text + context.SaltHashString);
        }

        public static string Hash(string text)
        {
            UTF8Encoding encoder = new();

            byte[] bytes = default;
            using (SHA256Managed hasher = new()) 
                bytes = hasher.ComputeHash(encoder.GetBytes(text));

            StringBuilder sb = new(bytes.Length);
            for (int i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("X2"));

            return sb.ToString();
        }
    }
}
