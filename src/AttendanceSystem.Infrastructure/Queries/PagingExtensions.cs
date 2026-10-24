using AttendanceSystem.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Infrastructure.Queries;

internal static class PagingExtensions
{
    /// <summary>Counts the whole query, then fetches one page of it. The query must already be ordered.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PageRequest page, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, page.Page, page.PageSize, totalCount);
    }
}
