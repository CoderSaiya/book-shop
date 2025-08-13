using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Domain.Entities;

public class Book
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Guid AuthorId { get; set; }
    [ForeignKey("AuthorId")]
    public Author Author { get; set; } = null!;
    [Required]
    public Guid PublisherId { get; set; }
    [ForeignKey("PublisherId")]
    public Publisher Publisher { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    [MinLength(5)]
    public string Title { get; set; } = null!;
    [MinLength(10)]
    public string? Description { get; set; }
    public int Stock { get; set; } = 0;
    [Required]
    public string[] CoverImage { get; set; } = null!;
    [Required]
    public DateTime PublishedDate { get; set; } = DateTime.Now;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [ForeignKey("CategoryId")]
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    // Navigation properties
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
}