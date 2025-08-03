using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookShop.Domain.ValueObjects;

namespace BookShop.Domain.Entities;

public class Profile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
    
    public Name? Name { get; set; }
    public Phone? Phone { get; set; }
    public Address? Address { get; set; }
    [Column(TypeName = "date")]
    public DateOnly? DateOfBirth { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}