using Hekucoreapp.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Hekucoreapp.Tests.Infrastructure.Logging;

// Minimal synthetic fixtures for BusinessLogPlannerTests. No real hekucoreapp entity implements
// IAggregateItem today (see IAggregateItem.cs's own comment — AppSettings/ContentItem/
// DeletedAccount/Person/StoredFile/UserRole were all judged to be their own aggregate root, none
// is a genuine single-hop "item of a root"), so these test-only types exercise
// BusinessLogPlanner's roll-up mechanism the same way gestamind's tests do with its real
// DiagnosticTest/DiagnosticTestItem/Observation entities — just with throwaway types instead,
// tracked by a tiny dedicated DbContext rather than the real HekucoreappDbContext.
public class TestRootEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestRootItemEntity : AuditableEntity, IAggregateItem
{
    public int Id { get; set; }
    public int TestRootEntityId { get; set; }

    string IAggregateItem.AggregateRootName => nameof(TestRootEntity);
    object IAggregateItem.AggregateRootId => TestRootEntityId;
}

// Deliberately named unrelated to its root (mirrors gestamind's Observation -> TestResult case):
// proves BusinessLogPlanner groups by the declared AggregateRootName/AggregateRootId, never by
// guessing from the item's own CLR type name.
public class NoteEntity : AuditableEntity, IAggregateItem
{
    public int Id { get; set; }
    public int OwningRootId { get; set; }

    string IAggregateItem.AggregateRootName => nameof(TestRootEntity);
    object IAggregateItem.AggregateRootId => OwningRootId;
}

public class BusinessLogPlannerTestDbContext : DbContext
{
    public DbSet<TestRootEntity> TestRoots => Set<TestRootEntity>();
    public DbSet<TestRootItemEntity> TestRootItems => Set<TestRootItemEntity>();
    public DbSet<NoteEntity> Notes => Set<NoteEntity>();

    public BusinessLogPlannerTestDbContext(DbContextOptions<BusinessLogPlannerTestDbContext> options)
        : base(options)
    {
    }
}
