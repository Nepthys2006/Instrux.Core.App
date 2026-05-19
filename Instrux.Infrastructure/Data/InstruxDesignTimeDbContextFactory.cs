using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Instrux.Infrastructure.Data;

public sealed class InstruxDesignTimeDbContextFactory : IDesignTimeDbContextFactory<InstruxDbContext>
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=InstruxDbLocal;Trusted_Connection=True;TrustServerCertificate=True;";

    public InstruxDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InstruxDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new InstruxDbContext(options);
    }
}
