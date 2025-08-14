namespace BookShop.Domain.Interfaces;

public interface IEntityLocalizer
{
    Task<string> LocalizeFieldAsync(
        string entityType, string entityKey, string field,
        string sourceText, string sourceLang, string targetLang);
}