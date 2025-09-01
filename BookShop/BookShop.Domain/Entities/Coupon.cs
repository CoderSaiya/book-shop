using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Domain.ValueObjects;

namespace BookShop.Domain.Entities;

public class Coupon
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(32)]
    public string Code { get; set; } = null!; // duy nhất theo (UserId, Code)

    public CouponType Type { get; set; } = CouponType.Percentage;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } // % (0–100) hoặc số tiền

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinSubtotal { get; set; } // đơn tối thiểu (client sẽ truyền subtotal để validate)

    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    [MaxLength(128)]
    public string? UsedContext { get; set; }

    public bool IsActive { get; set; } = true;

    [Timestamp]
    public byte[]? RowVersion { get; set; } // chống double-use
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
