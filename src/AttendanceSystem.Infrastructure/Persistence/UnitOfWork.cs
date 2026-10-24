using AttendanceSystem.Application.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Persistence;

/// <summary>One <c>SaveChangesAsync</c> per use case, which EF runs in a single transaction.</summary>
internal sealed class UnitOfWork(AttendanceDbContext db) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return ConcurrentChange();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation })
        {
            return ConcurrentChange();
        }
    }

    private static Error ConcurrentChange() =>
        Error.Conflict("concurrent_change", "Another request changed the same data at the same time. Retry the request.");
}
