using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PSFR_Repository.Models;
using System.Security.Claims;

namespace PSFR_DMS_CustomApis.Startup_Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, ConfigurationManager configuration)
        {    // Extract authentication settings
            string authorityUrl = configuration["IdentityServer:Url"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Missing configuration: Authentication:Authority");

            string clientId = configuration["IdentityServer:ClientId"]
                ?? throw new InvalidOperationException("Missing configuration: Authentication:ClientId");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        ConfigureTokenValidation(options, authorityUrl);
                        ConfigureTokenEvents(options, clientId);
                    });

            return services;
        }

        private static void ConfigureTokenValidation(JwtBearerOptions options, string authorityUrl)
        {
            options.Authority = authorityUrl;
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authorityUrl,

                ValidateAudience = true,
                ValidAudience = "IdentityServerApi",

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        }

        private static void ConfigureTokenEvents(JwtBearerOptions options, string clientId)
        {
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    ClaimsIdentity? identity = context.Principal?.Identities.FirstOrDefault();
                    if (identity is null)
                        return Task.CompletedTask;

                    // Remove existing role claim if present
                    RemoveExistingRoleClaim(identity);

                    // Add role and roleId based on client-specific claim
                    AddClientRoleClaims(identity, clientId);

                    return Task.CompletedTask;
                }
            };
        }

        private static void RemoveExistingRoleClaim(ClaimsIdentity identity)
        {
            Claim? existingRole = identity.FindFirst(ClaimTypes.Role);
            if (existingRole != null)
                identity.RemoveClaim(existingRole);
        }

        private static void AddClientRoleClaims(ClaimsIdentity identity, string clientId)
        {
            foreach (Claim? claim in identity.Claims.Where(c => c.Type == "Clients"))
            {
                if (TryParseClient(claim.Value, out var client) && client.ClientId == clientId)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, client.Role));
                    identity.AddClaim(new Claim("RoleId", client.RoleId.ToString()));
                    break; // stop after first match
                }
            }
        }

        private static bool TryParseClient(string json, out UserApplicationRoleModel client)
        {
            client = null!;

            try
            {
                UserApplicationRoleModel? parsed = JsonConvert.DeserializeObject<UserApplicationRoleModel>(json);

                if (parsed is null) return false;

                client = parsed;
                return true;
            }
            catch { return false; }
        }
    }
}
