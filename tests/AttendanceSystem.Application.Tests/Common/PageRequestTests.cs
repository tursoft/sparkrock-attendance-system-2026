using AttendanceSystem.Application.Common;

namespace AttendanceSystem.Application.Tests.Common;

public class PageRequestTests
{
    [Fact]
    public void Missing_values_default_to_the_first_page_of_fifty()
    {
        var result = PageRequest.Create(page: null, pageSize: null);

        Assert.Equal(1, result.Value.Page);
        Assert.Equal(50, result.Value.PageSize);
    }

    [Fact]
    public void Out_of_range_values_are_validation_errors_keyed_by_field()
    {
        var result = PageRequest.Create(page: 0, pageSize: 201);

        Assert.Equal(ErrorKind.Validation, result.Error?.Kind);
        Assert.Equal(["page", "pageSize"], result.Error!.ValidationErrors.Keys.Order());
    }
}
