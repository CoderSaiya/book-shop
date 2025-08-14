using BookShop.Domain.Entities;

namespace BookShop.Domain.Interfaces;

public interface ITranslationStore
{
    Task<TranslationEntry?> FindAsync(
        string entityType, string entityKey, string field,
        string targetLang, string sourceHash);

    Task UpsertAsync(TranslationEntry entry);
}