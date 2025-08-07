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
    public decimal Price { get; set; } = 0;
    [Required]
    public string[] CoverImage { get; set; } = null!;
    [Required]
    public DateTime PublishedDate { get; set; } = DateTime.Now;
}