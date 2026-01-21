using PSFR_DMS_CustomApis.Startup_Extensions;
using PSFR_Repository.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigurationManager configuration = builder.Configuration;
        IServiceCollection services = builder.Services;

        services.AddControllers();

        services.AddAppDbContext(configuration);
        services.AddAppServices();

        services.AddCustomCors(configuration);

        services.AddCustomAuthentication(configuration);
        services.AddAuthorization();

        services.AddSwaggerDocumentation(configuration);

        services.Configure<ExternalApisOptions>(configuration.GetSection("ExternalApis"));
        services.PostConfigure<ExternalApisOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.DmsBaseUrl))
                throw new InvalidOperationException("ExternalApis:DmsBaseUrl is not configured.");

            options.DmsBaseUrl = options.DmsBaseUrl.TrimEnd('/');
        });

        WebApplication app = builder.Build();

        app.UseRouting();

        app.UseCustomCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwaggerDocumentation();

        app.MapControllers();

        app.Run();
    }
}