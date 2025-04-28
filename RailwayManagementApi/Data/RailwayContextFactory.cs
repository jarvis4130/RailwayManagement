using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using RailwayManagementApi.Data;
using System.IO;

namespace RailwayManagementApi
{
    public class RailwayContextFactory : IDesignTimeDbContextFactory<RailwayContext>
    {
        public RailwayContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<RailwayContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new RailwayContext(builder.Options);
        }
    }
}
