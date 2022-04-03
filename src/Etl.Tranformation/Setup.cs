using Etl.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etl.Tranformation
{
    public static class Setup
    {
        public static IServiceCollection AddEtlTransformation(this IServiceCollection services, IConfiguration configuration)
        {
            var etlSetting = configuration.GetSection("Etl").Get<EtlSetting>();

            var cryptorInfo = new CryptorInfo(etlSetting);
            services.AddSingleton(cryptorInfo);
            services.AddTransient<ICryptorInfo>(sp => cryptorInfo);

            return services;
        }
    }
}
