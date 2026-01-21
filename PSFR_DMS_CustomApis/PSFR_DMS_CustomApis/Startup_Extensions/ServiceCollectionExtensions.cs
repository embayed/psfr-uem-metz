using PSFR_EditInDesktop.Helpers;
using PSFR_EditInDesktop.Services;
using PSFR_Repository.Repository;
using PSFR_Repository.Repository.Interfaces;
using PSFR_Repository.Services;
using PSFR_Repository.Services.Interfaces;

namespace PSFR_DMS_CustomApis.Startup_Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<ILookupItemRepository, LookupItemRepository>();
            services.AddScoped<IExceptionLoggerRepository, ExceptionLoggerRepository>();

            services.AddScoped<ILookupItemService, LookupItemService>();
            services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
            services.AddScoped<IMetadataTreeService, MetadataTreeService>();
            services.AddScoped<IAccessRulesRepository, AccessRulesRepository>();

            services.AddScoped<IWebDavService, WebDavService>();

            services.AddHttpClient("default")
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        UseDefaultCredentials = false,
                        Credentials = null
                    });

            services.AddHttpContextAccessor();
            services.AddSingleton<WebDavStateHelper>();

            // Add background service for cleaning up orphaned temp files
            services.AddHostedService<TempFileCleanupService>();

            return services;
        }
    }
}
