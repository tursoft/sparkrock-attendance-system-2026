using AttendanceSystem.Domain.Schools;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Repositories;

public sealed class SchoolRepository(AttendanceDbContext db) : ISchoolRepository
{
    public async Task<School?> GetAsync(int schoolId, CancellationToken cancellationToken)
    {
        var school = await db.Schools
            .AsNoTracking()
            .Include(s => s.SchoolTerms)
            .SingleOrDefaultAsync(s => s.SchoolId == schoolId, cancellationToken);

        return school is null ? null : SchoolMapper.ToDomain(school);
    }
}
