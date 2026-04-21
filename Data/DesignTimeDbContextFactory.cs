using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitManager.Data;

/// <summary>
/// Allows EF Core tools to create a DbContext at design-time (for migrations)
/// without needing a running database.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(
            "Server=72.61.47.240;Port=3306;Database=FitManager_test;User=remoto;Password=0315061;",
            new MySqlServerVersion(new Version(8, 0, 0)));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
