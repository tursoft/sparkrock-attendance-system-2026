namespace AttendanceSystem.Application.Common;

/// <summary>One page of a list query, with the total count across all pages.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
