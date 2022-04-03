using System;

namespace Etl.Core
{
    public interface IInitialization
    {
        void Initialize(object args, IServiceProvider sp);
    }
}
