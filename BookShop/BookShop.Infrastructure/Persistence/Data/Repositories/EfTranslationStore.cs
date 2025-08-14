using BookShop.Domain.Interfaces;
using BookShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookShop.Infrastructure.Persistence.Data.Repositories;

public class EfTranslationStore(IDbContextFactory<AppDbContext> factory) : ITranslationStore
{
    public async Task<TranslationEntry?> FindAsync(
        string entityType, string entityKey, string field,
        string targetLang, string sourceHash)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Translations.AsNoTracking().FirstOrDefaultAsync(
            x => x.EntityType == entityType
                 && x.EntityKey == entityKey
                 && x.Field == field
                 && x.TargetLang == targetLang
                 && x.SourceHash == sourceHash);
    }

    public async Task UpsertAsync(TranslationEntry entry)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        
        // Try find existing by unique key
        var existing = await ctx.Translations.FirstOrDefaultAsync(x =>
            x.EntityType == entry.EntityType &&
            x.EntityKey == entry.EntityKey &&
            x.Field == entry.Field &&
            x.TargetLang == entry.TargetLang &&
            x.SourceHash == entry.SourceHash);

        if (existing is null)
        {
            ctx.Translations.Add(entry);
        }
        else
        {
            existing.Value = entry.Value;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }
        await ctx.SaveChangesAsync();
    }
}