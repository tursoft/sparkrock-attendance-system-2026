namespace AttendanceSystem.Application.Common;

/// <summary>Commits all pending changes of a use case in one transaction.</summary>
public interface IUnitOfWork
{
    /// <summary>Returns a <see cref="ErrorKind.Conflict"/> error when a concurrent change wins the race (#41).</summary>
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken);
}
