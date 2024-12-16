using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PoS_Placeholder.Server.Models;

namespace PoS_Placeholder.Server.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add all models
    public DbSet<User> Users { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariation> ProductVariations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductArchive> ProductsArchive { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceArchive> ServicesArchive { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Business)
            .WithMany(b => b.Orders)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(o => o.User)
            .WithMany(o => o.Appointments)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Service>()
            .HasOne(o => o.User)
            .WithMany(o => o.Services)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Service>()
            .HasOne(o => o.Business)
            .WithMany(b => b.Services)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceArchive>()
            .HasOne(o => o.Order)
            .WithMany(b => b.Services)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// dotnet ef migrations add "migrationName"
// dotnet ef database update
