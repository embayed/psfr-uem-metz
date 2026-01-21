using Intalio.DMS.Repository;
using Microsoft.EntityFrameworkCore;
using PSFR_Repository.Context;
using PSFR_Services.Utilitys;

namespace PSFR_DMS_CustomApis.Startup_Extensions
{
    public static class DbContextExtensions
    {
        public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration config)
        {
            string? dbType = config.GetValue<string>("DatabaseType");
            string dbConnectionString = DecryptConnectionString(config);

            if (dbType == "PostgreSQL")
            {
                services.AddDbContext<DMSCustomContext>(opt => opt.UseNpgsql(dbConnectionString));
                services.AddDbContext<DMSContext>(opt => opt.UseNpgsql(dbConnectionString));

            }
            else
            {
                services.AddDbContext<DMSCustomContext>(opt => opt.UseSqlServer(dbConnectionString));
                services.AddDbContext<DMSContext>(opt => opt.UseSqlServer(dbConnectionString));

            }
            return services;
        }

        private static string DecryptConnectionString(IConfiguration config)
        {
            string? connectionString = config.GetConnectionString("DbConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("The 'DbConnection' connection string is missing or empty.");

            bool isEncrypted = config.GetValue<bool>("IsConnectionStringEncrypted");

            if (isEncrypted)
            {
                return EncryptionUtility.Decrypt(connectionString);
            }

            return connectionString;
        }

    }
}