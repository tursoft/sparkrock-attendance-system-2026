using AttendanceSystem.Domain.Schools;
using Riok.Mapperly.Abstractions;
using SchoolEntity = AttendanceSystem.Infrastructure.Persistence.Entities.School;
using SchoolTermEntity = AttendanceSystem.Infrastructure.Persistence.Entities.SchoolTerm;

namespace AttendanceSystem.Infrastructure.Repositories;

/// <summary>Builds the <see cref="School"/> aggregate through its constructor, so its invariants run.</summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class SchoolMapper
{
    [MapProperty(nameof(SchoolEntity.SchoolId), "id")]
    [MapProperty(nameof(SchoolEntity.Active), "isActive")]
    [MapProperty(nameof(SchoolEntity.TimeZone), "timeZoneId")]
    [MapProperty(nameof(SchoolEntity.SchoolTerms), "terms")]
    public static partial School ToDomain(SchoolEntity school);

    [MapProperty(nameof(SchoolTermEntity.TermId), "id")]
    [MapProperty(nameof(SchoolTermEntity.TermName), "name")]
    private static partial Term ToDomain(SchoolTermEntity term);

    private static SchoolYear ToSchoolYear(string schoolYear) => SchoolYear.Parse(schoolYear);
}
