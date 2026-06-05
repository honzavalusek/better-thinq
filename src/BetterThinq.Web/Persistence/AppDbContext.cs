using BetterThinq.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace BetterThinq.Web.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Settings> Settings => Set<Settings>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Template> Templates => Set<Template>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Settings>().HasKey(s => s.Id);
        modelBuilder.Entity<Settings>().Property(s => s.Region).HasConversion<string>();

        modelBuilder.Entity<Device>().HasKey(d => d.Id);

        modelBuilder.Entity<Template>().HasKey(t => t.Id);
        modelBuilder.Entity<Template>().Property(t => t.Mode).HasConversion<string?>();
        modelBuilder.Entity<Template>().Property(t => t.FanSpeed).HasConversion<string?>();
    }
}
