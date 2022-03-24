using Etl.Core.Utils;

namespace Etl.Core.Transformation.Fields
{
    public class EncryptField : FieldBase<string>
    {
        protected override string Convert(string text, Context context)
            => string.IsNullOrWhiteSpace(text) || context == null || context.CryptorInfo.key == null
                ? null 
                : Cryptor.Encrypt(text, context.CryptorInfo.key, context.CryptorInfo.iv);
    }
}
