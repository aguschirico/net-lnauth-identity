using Microsoft.EntityFrameworkCore;

namespace NetCore.Identity.LnAuth.Api.Database;

public static class DatabaseInitializer
{
    public static async Task RunMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}