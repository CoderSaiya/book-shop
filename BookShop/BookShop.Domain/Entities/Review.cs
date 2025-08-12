using System.ComponentModel.DataAnnotations;

namespace BookShop.Domain.Entities
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid BookId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsVerifiedPurchase { get; set; } = false;
        
        public int HelpfulCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
