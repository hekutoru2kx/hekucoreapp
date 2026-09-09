using Hekucoreapp.Domain.Common;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hekucoreapp.Infrastructure.Data;

public class HekucoreappDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbSet<AppInfo> AppInfos => Set<AppInfo>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<DeletedAccount> DeletedAccounts => Set<DeletedAccount>();
    // Named distinctly from the inherited IdentityDbContext.UserRoles (DbSet<IdentityUserRole<string>>,
    // backing AspNetUserRoles) to avoid silently shadowing it — this is a different table entirely.
    public DbSet<UserRole> UserRoleAssignments => Set<UserRole>();

    // Geographic reference data: Countries States Cities Database
    // https://github.com/dr5hn/countries-states-cities-database | ODbL v1.0
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();

    public HekucoreappDbContext(DbContextOptions<HekucoreappDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
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

        // AppSettings — singleton row (Id = 1), seeded by AppSettingsSeeder.
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(s => s.Id);
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

        return base.SaveChangesAsync(cancellationToken);
    }
}