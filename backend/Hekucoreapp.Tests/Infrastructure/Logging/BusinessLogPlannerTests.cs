using Hekucoreapp.Domain.Common;
using Hekucoreapp.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Hekucoreapp.Tests.Infrastructure.Logging;

public class BusinessLogPlannerTests
{
    private static BusinessLogPlannerTestDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BusinessLogPlannerTestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new BusinessLogPlannerTestDbContext(options);
    }

    private static BusinessLogPlanner.EntrySnapshot Snapshot(BusinessLogPlannerTestDbContext context, object entity)
    {
        var entry = context.Entry(entity);
        return new BusinessLogPlanner.EntrySnapshot(entry, entry.State, entity.GetType().Name, entity as IAggregateItem);
    }

    [Fact]
    public void Plan_AddedRootEntity_ProducesCreatedLine()
    {
        using var context = CreateContext(nameof(Plan_AddedRootEntity_ProducesCreatedLine));
        var root = new TestRootEntity { Id = 7, Name = "Alpha" };
        context.Add(root);

        var lines = BusinessLogPlanner.Plan([Snapshot(context, root)]);

        Assert.Single(lines);
        Assert.Equal("TestRootEntity 7 created", lines[0]);
    }

    [Fact]
    public void Plan_ModifiedRootEntity_ProducesUpdatedLine()
    {
        using var context = CreateContext(nameof(Plan_ModifiedRootEntity_ProducesUpdatedLine));
        var root = new TestRootEntity { Id = 12, Name = "Beta" };
        context.Attach(root);
        context.Entry(root).State = EntityState.Modified;

        var lines = BusinessLogPlanner.Plan([Snapshot(context, root)]);

        Assert.Single(lines);
        Assert.Equal("TestRootEntity 12 updated", lines[0]);
    }

    [Fact]
    public void Plan_DeletedRootEntity_ProducesDeletedLine()
    {
        using var context = CreateContext(nameof(Plan_DeletedRootEntity_ProducesDeletedLine));
        var root = new TestRootEntity { Id = 3, Name = "Gamma" };
        context.Attach(root);
        context.Entry(root).State = EntityState.Deleted;

        var lines = BusinessLogPlanner.Plan([Snapshot(context, root)]);

        Assert.Single(lines);
        Assert.Equal("TestRootEntity 3 deleted", lines[0]);
    }

    [Fact]
    public void Plan_MultipleItemsUnderSameRoot_ProducesOneRollUpLine()
    {
        using var context = CreateContext(nameof(Plan_MultipleItemsUnderSameRoot_ProducesOneRollUpLine));
        var item1 = new TestRootItemEntity { Id = 1, TestRootEntityId = 7 };
        var item2 = new TestRootItemEntity { Id = 2, TestRootEntityId = 7 };
        context.Add(item1);
        context.Attach(item2);
        context.Entry(item2).State = EntityState.Deleted;

        var lines = BusinessLogPlanner.Plan([Snapshot(context, item1), Snapshot(context, item2)]);

        Assert.Single(lines);
        Assert.Equal("TestRootEntity 7 items updated", lines[0]);
    }

    [Fact]
    public void Plan_ItemsUnderDifferentRoots_ProducesOneLinePerRoot()
    {
        using var context = CreateContext(nameof(Plan_ItemsUnderDifferentRoots_ProducesOneLinePerRoot));
        var item1 = new TestRootItemEntity { Id = 1, TestRootEntityId = 7 };
        var item2 = new TestRootItemEntity { Id = 2, TestRootEntityId = 9 };
        context.Add(item1);
        context.Add(item2);

        var lines = BusinessLogPlanner.Plan([Snapshot(context, item1), Snapshot(context, item2)]);

        Assert.Equal(2, lines.Count);
        Assert.Contains("TestRootEntity 7 items updated", lines);
        Assert.Contains("TestRootEntity 9 items updated", lines);
    }

    [Fact]
    public void Plan_MixOfRootAndItemChanges_ProducesBothKindsOfLines()
    {
        using var context = CreateContext(nameof(Plan_MixOfRootAndItemChanges_ProducesBothKindsOfLines));
        var root = new TestRootEntity { Id = 5, Name = "Delta" };
        var item = new TestRootItemEntity { Id = 1, TestRootEntityId = 5 };
        context.Add(root);
        context.Add(item);

        var lines = BusinessLogPlanner.Plan([Snapshot(context, root), Snapshot(context, item)]);

        Assert.Equal(2, lines.Count);
        Assert.Contains("TestRootEntity 5 created", lines);
        Assert.Contains("TestRootEntity 5 items updated", lines);
    }

    // Mirrors gestamind's "Observation rolls up to TestResult, not to a type named after itself"
    // case: NoteEntity's own class name has nothing to do with TestRootEntity, proving the planner
    // groups by the declared AggregateRootName/AggregateRootId, never by inferring from the
    // item's own CLR type name.
    [Fact]
    public void Plan_NoteItem_RollsUpToDeclaredRootNotItsOwnTypeName()
    {
        using var context = CreateContext(nameof(Plan_NoteItem_RollsUpToDeclaredRootNotItsOwnTypeName));
        var note = new NoteEntity { Id = 1, OwningRootId = 44 };
        context.Add(note);

        var lines = BusinessLogPlanner.Plan([Snapshot(context, note)]);

        Assert.Single(lines);
        Assert.Equal("TestRootEntity 44 items updated", lines[0]);
    }

    [Fact]
    public void Plan_NoChanges_ProducesNoLines()
    {
        var lines = BusinessLogPlanner.Plan([]);

        Assert.Empty(lines);
    }
}
