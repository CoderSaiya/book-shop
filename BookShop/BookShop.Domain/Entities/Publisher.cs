using System.ComponentModel.DataAnnotations;
using BookShop.Domain.ValueObjects;

namespace BookShop.Domain.Entities;

public class Publisher
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public Address Address { get; set; } = null!;
    [Required]
    [Url]
    public string Website { get; set; } = null!;
    
    public ICollection<Book> Books { get; set; } = new List<Book>();
}