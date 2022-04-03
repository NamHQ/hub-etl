using Etl.Core.Utils;

namespace Etl.Core.Transformation.Actions
{
    public class EncryptAction : TransformAction<EncryptActionInst>
    {
    }

    public class EncryptActionInst : TransformActionInst<string>
    {
        private readonly ICryptorInfo _crytorInfo;

        public EncryptActionInst(ICryptorInfo cryptorInfo)
        {
            _crytorInfo = cryptorInfo;
        }

        protected override string Execute(object input, ActionArgs args)
        {
            var text = input as string;

            return string.IsNullOrWhiteSpace(text) || _crytorInfo == null || _crytorInfo.Config.key == null
             ? null
             : Cryptor.Encrypt(text, _crytorInfo.Config.key, _crytorInfo.Config.iv);
        }
    }

}

