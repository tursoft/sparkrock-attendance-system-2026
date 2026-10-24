using AttendanceSystem.Application.Common;

namespace AttendanceSystem.Application.Tests.Common;

public class PageRequestTests
{
    [Fact]
    public void Missing_values_default_to_the_first_page_of_fifty()
    {
        var result = PageRequest.Create(page: null, pageSize: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(50, result.Value.PageSize);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 200)]
    [InlineData(int.MaxValue, 50)]
    public void Values_in_range_are_kept(int page, int pageSize)
    {
        var result = PageRequest.Create(page, pageSize);

        Assert.True(result.IsSuccess);
        Assert.Equal(page, result.Value.Page);
        Assert.Equal(pageSize, result.Value.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void A_page_below_one_is_a_validation_error(int page)
    {
        var result = PageRequest.Create(page, pageSize: null);

        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
        Assert.Equal(["page"], result.Error!.ValidationErrors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(201)]
    public void A_page_size_outside_one_to_two_hundred_is_a_validation_error(int pageSize)
    {
        var result = PageRequest.Create(page: null, pageSize);

        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
        Assert.Equal(["pageSize"], result.Error!.ValidationErrors.Keys);
    }

    [Fact]
    public void Every_invalid_value_is_reported_at_once()
    {
        var result = PageRequest.Create(page: 0, pageSize: 201);

        Assert.Equal(["page", "pageSize"], result.Error!.ValidationErrors.Keys.Order());
    }

    [Theory]
    [InlineData(1, 50, 0)]
    [InlineData(3, 50, 100)]
    [InlineData(2, 200, 200)]
    public void Skip_counts_the_items_on_earlier_pages(int page, int pageSize, int expected)
    {
        var request = PageRequest.Create(page, pageSize).Value;

        Assert.Equal(expected, request.Skip);
    }

    [Fact]
    public void Skip_saturates_instead_of_overflowing_so_a_huge_page_is_just_empty()
    {
        var request = PageRequest.Create(int.MaxValue, PageRequest.MaxPageSize).Value;

        Assert.Equal(int.MaxValue, request.Skip);
    }

    [Fact]
    public void Paged_result_carries_items_and_paging_metadata()
    {
        var paged = new PagedResult<string>(["a", "b"], Page: 2, PageSize: 2, TotalCount: 5);

        Assert.Equal(["a", "b"], paged.Items);
        Assert.Equal(2, paged.Page);
        Assert.Equal(2, paged.PageSize);
        Assert.Equal(5, paged.TotalCount);
    }
}
