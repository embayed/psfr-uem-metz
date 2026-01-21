using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using PSFR_DMS_CustomApis.Attributes;
using System.Reflection;

namespace PSFR_DMS_CustomApis.Startup_Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
        {
            bool hideOptionalControllers = configuration.GetValue<bool>("SwaggerSettings:HideOptionalControllers");

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "PSFR_DMS API", Version = "V1" });

                // JWT Authentication setup
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
            });

                // Conditionally hide controllers or methods based on attributes and config
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (apiDesc.ActionDescriptor is not ControllerActionDescriptor controller) return true;

                    bool controllerHidden = controller.ControllerTypeInfo
                        .GetCustomAttribute<HideClassFromSwaggerIfConfiguredAttribute>() != null;

                    bool methodHidden = controller.MethodInfo
                        .GetCustomAttribute<HideMethodFromSwaggerIfConfiguredAttribute>() != null;

                    return !hideOptionalControllers || (!controllerHidden && !methodHidden);
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PSFR_DMS API v1");
                c.RoutePrefix = "";
            });

            return app;
        }
    }
}
