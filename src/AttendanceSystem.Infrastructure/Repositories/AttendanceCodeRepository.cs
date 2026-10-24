using AttendanceSystem.Domain.Attendance;
using AttendanceSystem.Infrastructure.Mapping;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories;

/// <remarks>Values match case-insensitively through the database's default collation, as in the legacy system (#54).</remarks>
internal sealed class AttendanceCodeRepository(AttendanceDbContext db) : IAttendanceCodeRepository
{
    public async Task<IReadOnlyList<AttendanceCode>> GetManyAsync(IReadOnlyCollection<string> codeValues, CancellationToken cancellationToken)
    {
        var codes = await db.AttendanceCodes
            .AsNoTracking()
            .Where(c => codeValues.Contains(c.CodeValue))
            .ToListAsync(cancellationToken);
        return [.. codes.Select(c => c.ToDomain())];
    }
}
