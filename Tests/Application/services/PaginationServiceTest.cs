using Application.services;
using Domain.entities.search;

namespace Tests.Application.services;

[TestFixture]
[TestOf(typeof(PaginationService))]
public class PaginationServiceTest
{

    [Test]
    public void Paginate_InvalidPageNumberAndPageSize_UsesDefaultValues()
    {
        List<int> items = [1, 2, 3];

        SearchSettings settings = new ImageSearchSettings
        {
            PageNumber = 0,
            PageSize = 0
        };

        // Execution
        PaginatedResult<int> result = PaginationService.Paginate(items, settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(20));
            Assert.That(result.Items, Is.EqualTo(items));
        }
    }

    [Test]
    public void Paginate_PageNumberAboveTotalPages_UsesLastPage()
    {
        List<int> items = [1, 2, 3, 4, 5];

        SearchSettings settings = new ImageSearchSettings
        {
            PageNumber = 10,
            PageSize = 2
        };

        // Execution
        PaginatedResult<int> result = PaginationService.Paginate(items, settings);

        // Asserts
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PageNumber, Is.EqualTo(3));
            Assert.That(result.TotalPages, Is.EqualTo(3));
            Assert.That(result.Items, Is.EqualTo(new List<int>
        {
            5
        }));
        }
    }

}