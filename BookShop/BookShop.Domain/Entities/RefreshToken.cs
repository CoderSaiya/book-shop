using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Domain.Entities;

public class RefreshToken
{
    public Guid UserId { get; init; }
    [ForeignKey("UserId")]
    public User User { get; init; } = null!;

    [Required]
    public string Token { get; init; } = null!;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
}