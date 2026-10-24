using System.Text.RegularExpressions;

namespace AttendanceSystem.Api.IntegrationTests.Fixtures;

/// <summary>Splits sqlcmd-style scripts into batches, as sqlcmd does with <c>GO</c> lines.</summary>
public static partial class SqlScript
{
    public static IReadOnlyList<string> SplitBatches(string script) =>
        BatchSeparator().Split(script).Where(batch => !string.IsNullOrWhiteSpace(batch)).ToList();

    [GeneratedRegex(@"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex BatchSeparator();
}
