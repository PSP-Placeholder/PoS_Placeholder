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
    public DbSet<TaxArchive> TaxesArchive { get; set; }
    public DbSet<DiscountArchive> DiscountsArchives { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<UserWorkTime> UserWorkTimes { get; set; }
    public DbSet<Giftcard> Giftcards { get; set; }
    public DbSet<PaymentArchive> PaymentsArchive  { get; set; }

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
    }
}

// dotnet ef migrations add "migrationName"
// dotnet ef database update
