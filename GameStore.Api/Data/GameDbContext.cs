using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {

    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>()
            .HasOne(game => game.Category)
            .WithMany(category => category.Games)
            .HasForeignKey(game => game.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<Order>()
            .HasOne(order => order.User)
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .HasOne(item => item.Game)
            .WithMany()
            .HasForeignKey(item => item.GameId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<OrderItem>()
            .HasOne<Order>()
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(user => user.Balance)
            .HasPrecision(MoneyHelper.Precision, MoneyHelper.Scale);
        modelBuilder.Entity<Game>()
            .Property(game => game.Price)
            .HasPrecision(MoneyHelper.Precision, MoneyHelper.Scale);
        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(MoneyHelper.Precision, MoneyHelper.Scale);
    }
}