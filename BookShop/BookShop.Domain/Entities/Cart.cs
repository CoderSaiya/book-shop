using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Domain.Entities
{
    public class Cart
    {
        [Key]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Calculated property
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
    }
}