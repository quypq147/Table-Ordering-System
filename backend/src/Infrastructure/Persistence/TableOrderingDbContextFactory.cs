using Application.Abstractions;
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

        // During design-time, we don't need real dispatching. Use a no-op dispatcher.
        var dispatcher = new NoOpDispatcher();

        return new TableOrderingDbContext(options, dispatcher);
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<Domain.Abstractions.IDomainEvent> domainEvents, CancellationToken ct = default) => Task.CompletedTask;
    }
}

