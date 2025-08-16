using BookShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BookShop.Domain.Views;

namespace BookShop.Infrastructure.Persistence.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BookSearch> BookSearches { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<TranslationEntry> Translations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Constraint
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasDiscriminator<string>("Role")
                .HasValue<Client>("Client")
                .HasValue<Admin>("Admin");
            
            entity.OwnsOne(u => u.Email, e =>
            {
                e.Property(p => p.Address)
                    .HasColumnName("Email")
                    .HasMaxLength(256)
                    .IsRequired();

                e.HasIndex(p => p.Address).IsUnique();
            });
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_User_CreatedAt");
        });
        
        modelBuilder.Entity<Book>()
            .Property(b => b.CoverImage)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");
        
        modelBuilder.Entity<BookSearch>(eb =>
        {
            eb.HasNoKey();
            eb.ToView("vwBookSearch");
        });
        
        modelBuilder.Entity<RefreshToken>()
            .Property(r => r.IsRevoked)
            .HasDefaultValue(false);
        
        modelBuilder.Entity<Profile>(b =>
        {
            b.OwnsOne(p => p.Name);
            b.OwnsOne(p => p.Phone);
            b.OwnsOne(p => p.Address);
            
            b.HasIndex(e => e.DateOfBirth)
                .HasDatabaseName("IX_User_DateOfBirth");
        });
        
        modelBuilder.Entity<Publisher>()
            .OwnsOne(p => p.Address);
        
        // Relations
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RefreshToken>()
            .HasKey(r => new { r.UserId, r.Token });
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart)
            .WithOne(c => c.User)
            .HasForeignKey<Cart>(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(c => c.UserId);
            e.Property(c => c.IsActive).HasDefaultValue(true);
        });
        
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(ci => new { ci.CartId, ci.BookId });

            e.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .HasPrincipalKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ci => ci.Book)
                .WithMany() 
                .HasForeignKey(ci => ci.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(ci => ci.UnitPrice).HasColumnType("decimal(18,2)");
        });
            
        // modelBuilder.Entity<CartItem>()
        //     .HasOne(ci => ci.Cart)
        //     .WithMany(c => c.CartItems)
        //     .HasForeignKey(ci => ci.CartId)
        //     .OnDelete(DeleteBehavior.Cascade);
        //     
        // modelBuilder.Entity<CartItem>()
        //     .HasOne(ci => ci.Book)
        //     .WithMany(b => b.CartItems)
        //     .HasForeignKey(ci => ci.BookId)
        //     .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Book)
            .WithMany(b => b.OrderItems)
            .HasForeignKey(oi => oi.BookId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Index configurations
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Order_CreatedAt");
            
        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.BookId })
            .IsUnique()
            .HasDatabaseName("IX_Review_User_Book");
        
        modelBuilder.Entity<TranslationEntry>()
            .HasIndex(x => new { x.EntityType, x.EntityKey, x.Field, x.TargetLang, x.SourceHash })
            .IsUnique();
    }
}