using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Domain.Entities
{
    public class CartItem
    {
        [Required]
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        [Required]
        public Guid BookId { get; set; }
        [ForeignKey("BookId")]
        public Book Book { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Calculated property
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}