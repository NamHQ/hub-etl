namespace Etl.Core.Transformation
{
    public class Context
    {
        public readonly string TenantId;

        public readonly (byte[] key, byte[] iv) CryptorInfo;

        public readonly string SaltHashString;

        public Context(string tenantId, byte[] encryptKey, byte[] encryptIv, string saltHashString)
        {
            this.TenantId = tenantId;
            this.CryptorInfo = (encryptKey, encryptIv);
            this.SaltHashString = saltHashString;
        }
    }
}
