using AttendanceSystem.Infrastructure.Persistence;
using AttendanceSystem.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Api.IntegrationTests.Fixtures;

/// <summary>
/// Seeds and reads rows directly through EF, each call on a fresh context.
/// Every test creates its own schools, students and codes, so tests stay independent and can run in parallel.
/// </summary>
public sealed class TestData(DatabaseFixture database)
{
    private static int s_codeCounter;

    public Task<School> CreateSchoolAsync(string timeZone = "UTC", int? absenceAlertThreshold = null, bool active = true) =>
        InsertAsync(new School
        {
            SchoolName = $"Test School {Guid.NewGuid():N}",
            TimeZone = timeZone,
            AbsenceAlertThreshold = absenceAlertThreshold,
            Active = active,
        });

    public Task<SchoolTerm> AddTermAsync(int schoolId, string schoolYear, DateOnly startDate, DateOnly endDate, string? termName = null) =>
        InsertAsync(new SchoolTerm
        {
            SchoolId = schoolId,
            SchoolYear = schoolYear,
            TermName = termName ?? $"Term from {startDate:yyyy-MM-dd}",
            StartDate = startDate,
            EndDate = endDate,
        });

    public Task<Student> CreateStudentAsync(int schoolId, bool active = true, string? grade = null) =>
        InsertAsync(new Student
        {
            SchoolId = schoolId,
            FirstName = "Test",
            LastName = $"Student {Guid.NewGuid():N}",
            Grade = grade,
            Active = active,
        });

    /// <summary>Adds a code with a unique value, so tests never change the shared seeded codes.</summary>
    public Task<AttendanceCode> CreateAttendanceCodeAsync(bool isAbsent = false, bool isExcused = false, bool isActive = true)
    {
        var value = $"T{Interlocked.Increment(ref s_codeCounter):X4}";
        return InsertAsync(new AttendanceCode
        {
            CodeValue = value,
            Description = $"Test code {value}",
            IsAbsent = isAbsent,
            IsExcused = isExcused,
            IsActive = isActive,
        });
    }

    /// <summary>Inserts any row; the returned entity carries its generated keys.</summary>
    public async Task<TEntity> InsertAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await using var db = database.CreateDbContext();
        db.Add(entity);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return entity;
    }

    /// <summary>Reads persisted state with a fresh, non-tracking context.</summary>
    public async Task<TResult> QueryAsync<TResult>(Func<AttendanceDbContext, Task<TResult>> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var db = database.CreateDbContext();
        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return await query(db);
    }
}
