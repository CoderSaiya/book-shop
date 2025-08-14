using BookShop.Application.Interface;
using BookShop.Domain.Entities;
using BookShop.Domain.Interfaces;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class EntityLocalizer(
    ITextHasher hasher,
    ITranslationStore store,
    ITranslator translator) : IEntityLocalizer
{
    public async Task<string> LocalizeFieldAsync(
        string entityType, string entityKey, string field,
        string sourceText, string sourceLang, string targetLang)
    {
        if (string.IsNullOrWhiteSpace(sourceText)) return sourceText ?? "";

        var sourceHash = hasher.ComputeHash(sourceText.Trim());
        var cached = await store.FindAsync(entityType, entityKey, field, targetLang, sourceHash);
        if (cached is not null) return cached.Value;

        // Gọi dịch máy
        var translated = await translator.TranslateAsync(sourceText, sourceLang, targetLang);
        // Lưu cache
        await store.UpsertAsync(new TranslationEntry
        {
            EntityType = entityType,
            EntityKey = entityKey,
            Field = field,
            SourceLang = sourceLang,
            TargetLang = targetLang,
            SourceHash = sourceHash,
            Value = translated,
            UpdatedAt = DateTimeOffset.UtcNow,
        });

        return translated;
    }
}