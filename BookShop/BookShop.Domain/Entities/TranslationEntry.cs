using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShop.Domain.Entities;

[Table("Translations")]
public class TranslationEntry
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(64)] public string EntityType { get; set; } = null!;
    [Required, MaxLength(128)] public string EntityKey { get; set; } = null!; // slug or Id
    [Required, MaxLength(64)] public string Field { get; set; } = null!;

    [Required, MaxLength(8)] public string SourceLang { get; set; } = "vi";
    [Required, MaxLength(8)] public string TargetLang { get; set; } = "en";

    // Hash của text gốc để biết khi nào nội dung đã thay đổi
    [Required, MaxLength(64)] public string SourceHash { get; set; } = null!; // SHA-256 hex

    [Required] public string Value { get; set; } = null!; // Bản dịch
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}