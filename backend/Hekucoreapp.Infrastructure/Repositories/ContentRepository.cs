using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Domain.Enums;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hekucoreapp.Infrastructure.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly HekucoreappDbContext _context;

    public ContentRepository(HekucoreappDbContext context)
    {
        _context = context;
    }

    public async Task<ContentItem?> GetByIdAsync(int id) =>
        await _context.ContentItems.Include(c => c.StoredFile).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ContentItem?> GetSlotAsync(string ownerType, int ownerId, string slot) =>
        await _context.ContentItems.Include(c => c.StoredFile)
            .FirstOrDefaultAsync(c => c.OwnerType == ownerType && c.OwnerId == ownerId && c.Slot == slot);

    public async Task<IReadOnlyList<ContentItem>> GetForOwnerAsync(string ownerType, int ownerId, bool includeUnpublished, bool includeArchived)
    {
        var query = _context.ContentItems.Include(c => c.StoredFile)
            .Where(c => c.OwnerType == ownerType && c.OwnerId == ownerId && c.Slot == null);

        if (!includeUnpublished) query = query.Where(c => c.PublishedAt != null);
        if (!includeArchived) query = query.Where(c => c.ArchivedAt == null);

        return await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToListAsync();
    }

    public async Task<(ContentItem Item, StoredFile? ReplacedFile)> UpsertFileSlotAsync(string ownerType, int ownerId, string slot, StoredFile file, ContentKind kind, string title)
    {
        var existing = await _context.ContentItems.Include(c => c.StoredFile)
            .FirstOrDefaultAsync(c => c.OwnerType == ownerType && c.OwnerId == ownerId && c.Slot == slot);

        StoredFile? replaced = null;

        if (existing != null)
        {
            replaced = existing.StoredFile;
            existing.Kind = kind;
            existing.Title = title;
            existing.Url = null;
            existing.StoredFile = file;
            existing.PublishedAt ??= DateTime.UtcNow;
            if (replaced != null) _context.StoredFiles.Remove(replaced);
        }
        else
        {
            existing = new ContentItem
            {
                OwnerType = ownerType,
                OwnerId = ownerId,
                Slot = slot,
                Kind = kind,
                Title = title,
                StoredFile = file,
                PublishedAt = DateTime.UtcNow
            };
            _context.ContentItems.Add(existing);
        }

        await _context.SaveChangesAsync();
        return (existing, replaced);
    }

    public async Task<StoredFile?> DeleteSlotAsync(string ownerType, int ownerId, string slot)
    {
        var existing = await _context.ContentItems.Include(c => c.StoredFile)
            .FirstOrDefaultAsync(c => c.OwnerType == ownerType && c.OwnerId == ownerId && c.Slot == slot);
        if (existing == null) return null;

        var file = existing.StoredFile;
        _context.ContentItems.Remove(existing);
        if (file != null) _context.StoredFiles.Remove(file);
        await _context.SaveChangesAsync();

        return file;
    }
}
