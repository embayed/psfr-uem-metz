namespace PSFR_DMS_CustomApis.Startup_Extensions
{
    public static class CorsExtensions
    {
        private const string PolicyName = "AllowConfiguredOrigins";

        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
        {
            string[]? allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins ?? [])
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            app.UseCors(PolicyName);
            return app;
        }
    }
}
