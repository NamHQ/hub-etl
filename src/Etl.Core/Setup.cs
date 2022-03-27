using Etl.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etl.Core
{
    public static class Setup
    {
        public static IServiceCollection AddEtl(this IServiceCollection services, IConfiguration configuration)
        {
            var etlSetting = configuration.GetSection("Etl").Get<EtlSetting>();
            services.AddSingleton(etlSetting);

            services.AddSingleton<IEtlFactory>(new EtlFactory(etlSetting));

            var etlContext = new EtlContext(etlSetting);
            services.AddSingleton(etlContext);
            services.AddTransient<IEtlContext>(sp => etlContext);



            services.AddTransient<Workflow>();


            return services;
        }
    }
}
