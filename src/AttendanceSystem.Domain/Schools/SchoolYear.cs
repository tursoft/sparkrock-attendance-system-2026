using System.Diagnostics.CodeAnalysis;

namespace AttendanceSystem.Domain.Schools;

/// <summary>A school year name in the form <c>YYYY-YYYY</c>, where the second year is the first plus one.</summary>
public sealed record SchoolYear
{
    private SchoolYear(int startYear) => StartYear = startYear;

    public int StartYear { get; }

    public int EndYear => StartYear + 1;

    public static bool TryParse(string? value, [NotNullWhen(true)] out SchoolYear? schoolYear)
    {
        schoolYear = null;
        if (value is not { Length: 9 } || value[4] != '-'
            || !TryParseYear(value.AsSpan(0, 4), out var startYear)
            || !TryParseYear(value.AsSpan(5, 4), out var endYear)
            || startYear < 1 || endYear != startYear + 1)
        {
            return false;
        }

        schoolYear = new SchoolYear(startYear);
        return true;
    }

    public static SchoolYear Parse(string value) =>
        TryParse(value, out var schoolYear)
            ? schoolYear
            : throw new DomainException($"'{value}' is not a valid school year. Expected YYYY-YYYY with consecutive years.");

    public override string ToString() => $"{StartYear:D4}-{EndYear:D4}";

    private static bool TryParseYear(ReadOnlySpan<char> digits, out int year)
    {
        year = 0;
        foreach (var digit in digits)
        {
            if (!char.IsAsciiDigit(digit))
            {
                return false;
            }

            year = (year * 10) + (digit - '0');
        }

        return true;
    }
}
