using System.ComponentModel.DataAnnotations;
using BookShop.Domain.ValueObjects;

namespace BookShop.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Email Email { get; set; } = null!;
    [Required]
    [MaxLength(256)]
    public string Password { get; set; } = null!;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public Profile Profile { get; set; } = new();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}