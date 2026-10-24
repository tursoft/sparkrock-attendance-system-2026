using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Api.IntegrationTests.Fixtures;

public sealed class DatabaseFixtureTests(DatabaseFixture database)
{
    [Fact]
    public async Task Scripts_ran_and_reference_codes_are_seeded()
    {
        var codes = await new TestData(database).QueryAsync(db => db.AttendanceCodes
            .Where(c => c.CodeValue == "P" || c.CodeValue == "A" || c.CodeValue == "E" || c.CodeValue == "L")
            .OrderBy(c => c.CodeValue)
            .Select(c => new { c.CodeValue, c.IsAbsent, c.IsExcused, c.IsActive })
            .ToListAsync(TestContext.Current.CancellationToken));

        Assert.Equal(
            [("A", true, false), ("E", true, true), ("L", false, false), ("P", false, false)],
            codes.Select(c => (c.CodeValue, c.IsAbsent, c.IsExcused)));
        Assert.All(codes, c => Assert.True(c.IsActive));
    }
}
