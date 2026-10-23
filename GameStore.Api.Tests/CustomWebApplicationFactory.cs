using GameStore.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace GameStore.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<GameDbContext>>();
            services.RemoveAll<GameDbContext>();
            services.RemoveAll<DbContextOptions<GameDbContext>>();

            services.AddDbContext<GameDbContext>(options =>
                options.UseInMemoryDatabase($"GameStoreTests-{Guid.NewGuid()}"));
        });
    }
}