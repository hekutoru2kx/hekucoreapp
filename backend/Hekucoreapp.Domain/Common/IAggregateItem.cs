namespace Hekucoreapp.Domain.Common;

// Marks an entity as a child of some other aggregate root, purely for HekucoreappDbContext's
// business-CRUD logging (see BusinessLogPlanner): a changed IAggregateItem never gets its own log
// line, it folds into one "{Root} {Id} items updated" line per root per save. Explicit interface
// implementation on purpose — these two members exist only for that log-grouping code to read,
// not as part of the entity's normal public surface (and so EF's convention-based model discovery
// never mistakes them for a mapped column).
//
// No hekucoreapp entity implements this today (ported 2026-09-23): AppSettings/ContentItem/
// DeletedAccount/Person/StoredFile/UserRole were all judged to be their own root — none is a
// genuinely single-hop "item of a root" that floods logs with many rows per save the way
// gestamind's DiagnosticTestItem/Observation/HistoryFieldOption do. The interface still exists as
// infrastructure so BusinessLogPlanner's roll-up logic is exercised (see the test project) and so
// a future entity can opt in without any other change.
public interface IAggregateItem
{
    string AggregateRootName { get; }
    object AggregateRootId { get; }
}
