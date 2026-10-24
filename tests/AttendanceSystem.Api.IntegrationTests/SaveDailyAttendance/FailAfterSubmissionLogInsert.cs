using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AttendanceSystem.Api.IntegrationTests.SaveDailyAttendance;

/// <summary>
/// Fails the save after the command inserting the submission log has run, so earlier writes in the same
/// transaction have already reached the database and must be rolled back.
/// </summary>
internal sealed class FailAfterSubmissionLogInsert : DbCommandInterceptor
{
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("INSERT INTO [AttendanceSubmissionLog]", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Simulated failure after the submission log insert.");
        }

        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }
}
