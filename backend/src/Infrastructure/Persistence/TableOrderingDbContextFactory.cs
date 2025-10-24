using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class TableOrderingDbContextFactory : IDesignTimeDbContextFactory<TableOrderingDbContext>
{
    public TableOrderingDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine(basePath, "..", "Api", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine(basePath, "..", "Api", "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conn = cfg.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(conn))
        {
            // Fallback DEV cho migration
            conn = "Server=.;Database=TableOrderingDb;Trusted_Connection=True;TrustServerCertificate=True";
        }

        var options = new DbContextOptionsBuilder<TableOrderingDbContext>()
            .UseSqlServer(conn)
            .Options;

        return new TableOrderingDbContext(options);
    }
}

