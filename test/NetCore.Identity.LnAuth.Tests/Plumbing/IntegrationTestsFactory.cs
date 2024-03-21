using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetCore.Identity.LnAuth.Api.Configuration;
using NetCore.Identity.LnAuth.Api.Domain.Entities;
using Testcontainers.PostgreSql;

namespace NetCore.Identity.LnAuth.Tests.Plumbing;


public class IntegrationTestsFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:14.7")
        .WithDatabase("lightningauth-db")
        .WithUsername("admin")
        .WithPassword("Test.123")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<TDbContext>();
            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(_container.GetConnectionString());
            });
            services.AddSingleton(new AuthSettings
            {
                Issuer = "test",
                Audience = "test",
                SecretKey = "123043924932fsofdsrenwifdnsfkdse09432032fidonfio234930",
                TokenExpireSeconds = 3600,
                RefreshTokenExpireSeconds = 3600,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            });
            services.AddSingleton(GetMockUserManager());
        });
    }
    private static Mock<UserManager<AppUser>> GetMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }
    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}