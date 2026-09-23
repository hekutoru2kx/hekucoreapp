using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Common;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Domain.Enums;
using Hekucoreapp.Infrastructure.Identity;
using Hekucoreapp.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hekucoreapp.Infrastructure.Data;

public class HekucoreappDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICategoryLogger _categoryLogger;

    public DbSet<AppInfo> AppInfos => Set<AppInfo>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    // Six fixed rows (one per LogCategory) plus a singleton retention row, see
    // LoggingCategorySettings/LoggingRetentionSettings.
    public DbSet<LoggingCategorySettings> LoggingCategorySettings => Set<LoggingCategorySettings>();
    public DbSet<LoggingRetentionSettings> LoggingRetentionSettings => Set<LoggingRetentionSettings>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<DeletedAccount> DeletedAccounts => Set<DeletedAccount>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    // Named distinctly from the inherited IdentityDbContext.UserRoles (DbSet<IdentityUserRole<string>>,
    // backing AspNetUserRoles) to avoid silently shadowing it — this is a different table entirely.
    public DbSet<UserRole> UserRoleAssignments => Set<UserRole>();

    // Geographic reference data: Countries States Cities Database
    // https://github.com/dr5hn/countries-states-cities-database | ODbL v1.0
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();

    public HekucoreappDbContext(DbContextOptions<HekucoreappDbContext> options, IHttpContextAccessor httpContextAccessor, ICategoryLogger categoryLogger)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _categoryLogger = categoryLogger;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AspNetUserRoles (Identity's built-in role-assignment join table) is fully replaced by
        // UserRole/UserRoleAssignments above — nothing in the app reads or writes it anymore.
        // Excluding it from the model (rather than just leaving it unused) drops the table via
        // migration and makes any accidental future call to UserManager.AddToRoleAsync/GetRolesAsync
        // fail loudly instead of silently writing to a table the app no longer honors.
        modelBuilder.Ignore<IdentityUserRole<string>>();

        // AppSettings — singleton row (Id = 1), seeded by AppSettingsSeeder. The Content* column
        // defaults below matter beyond documentation: they're what backfills the row that
        // already exists on every established install when this migration's AddColumn runs —
        // without them the ALTER TABLE would fall back to the CLR defaults (0 / ""), which for
        // ContentAllowedContentTypes means an empty allow-list that rejects every upload.
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.ContentMaxBytes).HasDefaultValue(5 * 1024 * 1024);
            entity.Property(s => s.ContentAllowedContentTypes).HasDefaultValue("image/jpeg,image/png,image/webp");
            entity.Property(s => s.ContentMaxImageDimension).HasDefaultValue(2048);
            entity.Property(s => s.ContentAvatarMaxDimension).HasDefaultValue(512);
        });

        // LoggingCategorySettings — six fixed rows (one per LogCategory), seeded by
        // LoggingSettingsSeeder, never created/deleted through the API.
        modelBuilder.Entity<LoggingCategorySettings>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Category).HasConversion<string>();
            entity.Property(s => s.MinimumLevel).HasConversion<string>();
            entity.HasIndex(s => s.Category).IsUnique();
        });

        // LoggingRetentionSettings — singleton row (Id = 1), seeded by LoggingSettingsSeeder.
        modelBuilder.Entity<LoggingRetentionSettings>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.RetentionDays).HasDefaultValue(30);
        });

        // Person
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => new { p.DocumentType, p.DocumentId })
                .IsUnique()
                .HasFilter("document_type IS NOT NULL AND document_id IS NOT NULL");
            entity.HasOne(p => p.Country)
                .WithMany()
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.State)
                .WithMany()
                .HasForeignKey(p => p.StateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.City)
                .WithMany()
                .HasForeignKey(p => p.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ContentItem — generic, polymorphic content/attachment item (see the entity's own
        // comment). OwnerType/OwnerId has no FK (deliberate — any future entity can be an
        // owner). Restrict on StoredFile so a stray direct delete can't silently orphan the
        // blob reference; ContentService always deletes the StoredFile row itself.
        modelBuilder.Entity<ContentItem>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.Body).HasColumnType("text");
            entity.Property(c => c.Url).HasMaxLength(2048);
            entity.HasIndex(c => new { c.OwnerType, c.OwnerId, c.DisplayOrder });
            entity.HasIndex(c => new { c.OwnerType, c.OwnerId, c.Slot })
                .IsUnique()
                .HasFilter("slot IS NOT NULL");
            entity.HasOne(c => c.StoredFile)
                .WithMany()
                .HasForeignKey(c => c.StoredFileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StoredFile — blob metadata only (see the entity's own comment).
        modelBuilder.Entity<StoredFile>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => f.Sha256);
        });

        // ApplicationUser → Person
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Person)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.PersonId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(u => u.PersonId)
                .IsUnique()
                .HasFilter("person_id IS NOT NULL");
        });

        // DeletedAccount
        modelBuilder.Entity<DeletedAccount>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.NormalizedEmail);
        });

        // UserRole — audit-trailed role assignment (grant/revoke history, scheduled Starts/Expires),
        // replacing plain AspNetUserRoles as the source of truth for "which roles does this user
        // hold" (see UserRoleRepository). Restrict rather than Cascade on both FKs so deleting a
        // role or user never silently erases its assignment history.
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => ur.Id);
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique()
                .HasFilter("revoked_at IS NULL");
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Country
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(c => c.Id);
        });

        // State
        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne(s => s.Country)
                .WithMany()
                .HasForeignKey(s => s.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // City
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.State)
                .WithMany()
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Every AuditableEntity gets its CreatedAt/UpdatedAt columns indexed, so
        // date-range queries stay fast as these tables grow — applies automatically
        // to future auditable entities too, no per-entity wiring needed.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType)) continue;

            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(AuditableEntity.CreatedAt));
            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(AuditableEntity.UpdatedAt));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
					entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }

        // Snapshot before the save — EntityState resets to Unchanged/Detached on success, so
        // State must be read now; the entries themselves stay valid to read from afterward, which
        // is when an Added row's real database-generated Id first becomes available.
        var businessLogSnapshots = ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new BusinessLogPlanner.EntrySnapshot(e, e.State, e.Entity.GetType().Name, e.Entity as IAggregateItem))
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var line in BusinessLogPlanner.Plan(businessLogSnapshots))
            _categoryLogger.Log(LogCategory.Business, LogLevel.Information, line);

        return result;
    }
}