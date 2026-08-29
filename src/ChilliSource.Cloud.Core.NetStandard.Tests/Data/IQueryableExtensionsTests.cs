using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests.Data;

public class IQueryableExtensionsTests
{
    private readonly StringBuilder Console = new StringBuilder();
    private readonly ITestOutputHelper _output;

    public IQueryableExtensionsTests(ITestOutputHelper output)
    {
        _output = output;

        using (var context = TestDbContext.Create())
        {
            context.Database.EnsureCreated();
        }
    }

    public void Dispose()
    {
        var outputStr = Console.ToString();
        if (outputStr.Length > 0)
        {
            _output.WriteLine(outputStr);
        }
    }

    [Fact]
    public void TestInMemoryPagedList()
    {
        var list = Enumerable.Range(0, 1000).ToList();

        var paged = list.AsQueryable().ToPagedList(page: 1, pageSize: 100);

        Assert.True(paged.TotalCount == 1000);
        Assert.True(paged.CurrentPage == 1);
        Assert.True(paged.PageSize == 100);
        Assert.True(paged.Count == 100);
    }

    [Fact]
    public async Task TestInMemoryPagedListAsync()
    {
        var list = Enumerable.Range(0, 1000).ToList();

        var paged = await list.AsQueryable().ToPagedListAsync(page: 1, pageSize: 100);

        Assert.True(paged.TotalCount == 1000);
        Assert.True(paged.CurrentPage == 1);
        Assert.True(paged.PageSize == 100);
        Assert.True(paged.Count == 100);
    }

    [Fact]
    public void TestDBPagedList()
    {
        using (var context = TestDbContext.Create())
        {
            var paged = context.DistributedLocks
                            .OrderBy(l => l.Id)
                            .ToPagedList(page: 1, pageSize: 100);

            Assert.True(paged.CurrentPage == 1);
            Assert.True(paged.PageSize <= 100);
        }
    }

    [Fact]
    public async Task TestDBPagedListAsync()
    {
        using (var context = TestDbContext.Create())
        {
            var paged = await context.DistributedLocks
                                .OrderBy(l => l.Id)
                                .ToPagedListAsync(page: 1, pageSize: 100);

            Assert.True(paged.CurrentPage == 1);
            Assert.True(paged.PageSize <= 100);
        }
    }

    private static IQueryable<int> Items(int count) => Enumerable.Range(0, count).AsQueryable();

    [Theory]
    // count, page, pageSize, expected TotalCount, expected PageCount, expected elements
    [InlineData(0, 1, 10, 0, 0, 0)]     // empty set
    [InlineData(5, 1, 10, 5, 1, 5)]     // short first page - total derived, no count query
    [InlineData(10, 1, 10, 10, 1, 10)]  // exactly one full page
    [InlineData(25, 1, 10, 25, 3, 10)]  // full first page
    [InlineData(25, 2, 10, 25, 3, 10)]  // full middle page
    [InlineData(25, 3, 10, 25, 3, 5)]   // short last page - total derived from offset
    [InlineData(30, 3, 10, 30, 3, 10)]  // full last page
    public void TestPagedListTotals(int count, int page, int pageSize, int expectedTotal, int expectedPageCount, int expectedElements)
    {
        var paged = Items(count).ToPagedList(page, pageSize);

        Assert.Equal(expectedTotal, paged.TotalCount);
        Assert.Equal(expectedPageCount, paged.PageCount);
        Assert.Equal(expectedElements, paged.Count);
        Assert.Equal(paged.TotalCount, paged.UnfilteredCount);
        Assert.Equal(pageSize, paged.PageSize);
    }

    [Fact]
    public void TestPagedListElementsAreTheRequestedPage()
    {
        var paged = Items(25).ToPagedList(page: 3, pageSize: 10);

        Assert.Equal(new[] { 20, 21, 22, 23, 24 }, paged);
        Assert.Equal(3, paged.CurrentPage);
    }

    [Fact]
    public void TestPagedListPastEndReturnsEmptyPage()
    {
        var paged = Items(25).ToPagedList(page: 5, pageSize: 10);

        Assert.Empty(paged);
        Assert.Equal(25, paged.TotalCount);
        Assert.Equal(5, paged.CurrentPage);
    }

    [Fact]
    public void TestPagedListPastEndFallsBackToLastPage()
    {
        var paged = Items(25).ToPagedList(page: 5, pageSize: 10, previousPageIfEmpty: true);

        Assert.Equal(new[] { 20, 21, 22, 23, 24 }, paged);
        Assert.Equal(25, paged.TotalCount);
        Assert.Equal(3, paged.CurrentPage);
    }

    [Fact]
    public void TestPagedListEmptySetWithPreviousPageIfEmpty()
    {
        var paged = Items(0).ToPagedList(page: 1, pageSize: 10, previousPageIfEmpty: true);

        Assert.Empty(paged);
        Assert.Equal(0, paged.TotalCount);
        Assert.Equal(1, paged.CurrentPage);
    }

    [Fact]
    public void TestPagedListUnboundedPageSize()
    {
        var paged = Items(1000).ToPagedList(page: 1, pageSize: int.MaxValue);

        Assert.Equal(1000, paged.TotalCount);
        Assert.Equal(1000, paged.Count);
        Assert.Equal(1, paged.PageCount);
        Assert.Equal(1, paged.CurrentPage);
    }

    [Fact]
    public async Task TestPagedListTotalsAsync()
    {
        var paged = await Items(25).ToPagedListAsync(page: 3, pageSize: 10);

        Assert.Equal(25, paged.TotalCount);
        Assert.Equal(25, paged.UnfilteredCount);
        Assert.Equal(5, paged.Count);
        Assert.Equal(3, paged.CurrentPage);
    }
}
