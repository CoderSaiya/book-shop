using System.ComponentModel.DataAnnotations;

namespace BookShop.Domain.Entities;

public class Author
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;
    [MinLength(10)]
    public string? Bio { get; set; }
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
}