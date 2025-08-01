using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LibraryAutomation.Data
{
        public class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
        {
            public LibraryDbContext CreateDbContext(string[] args)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var builder = new DbContextOptionsBuilder<LibraryDbContext>();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                builder.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 11, 11)));

                return new LibraryDbContext(builder.Options);
            }
        }
    }

