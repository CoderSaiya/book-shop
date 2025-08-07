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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Constraint
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasDiscriminator<string>("Role")
                .HasValue<User>("Client")
                .HasValue<Admin>("Admin");
            
            entity.OwnsOne(u => u.Email, owned =>
            {
                owned.Property(e => e.Address)
                    .HasColumnName("Email");
                
                owned.HasIndex(e => e.Address)
                    .HasDatabaseName("IX_User_Email");
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
            .HasForeignKey<Profile>(p => p.UserId);
        
        modelBuilder.Entity<RefreshToken>()
            .HasKey(r => new { r.UserId, r.Token });
    }
}