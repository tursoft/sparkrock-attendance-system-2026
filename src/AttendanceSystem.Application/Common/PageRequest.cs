namespace AttendanceSystem.Application.Common;

/// <summary>A validated page of a list query (#45, #50).</summary>
public sealed record PageRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    private PageRequest(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    /// <summary>Items on earlier pages. Saturates at <see cref="int.MaxValue"/>, so a huge page is simply past the end.</summary>
    public int Skip => (int)Math.Min((Page - 1L) * PageSize, int.MaxValue);

    /// <summary>Applies the defaults to missing values and reports every out-of-range value, keyed <c>page</c> and <c>pageSize</c>.</summary>
    public static Result<PageRequest> Create(int? page, int? pageSize)
    {
        var resolvedPage = page ?? DefaultPage;
        var resolvedPageSize = pageSize ?? DefaultPageSize;
        var errors = new Dictionary<string, string[]>();

        if (resolvedPage < 1)
        {
            errors["page"] = ["The page must be 1 or greater."];
        }

        if (resolvedPageSize is < 1 or > MaxPageSize)
        {
            errors["pageSize"] = [$"The page size must be between 1 and {MaxPageSize}."];
        }

        return errors.Count > 0
            ? Error.Validation(errors)
            : new PageRequest(resolvedPage, resolvedPageSize);
    }
}
