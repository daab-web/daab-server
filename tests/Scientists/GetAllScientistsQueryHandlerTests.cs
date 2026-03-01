using Daab.Modules.Scientists.Features.Scientists.GetAllScientists;
using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Tests;

public class GetAllScientistsQueryHandlerTests
{
    private static ScientistsDbContext CreateInMemoryContext()
    {
        var dbName = Path.GetTempFileName();
        var options = new DbContextOptionsBuilder<ScientistsDbContext>()
            .UseSqlite($"Data Source={dbName}")
            .Options;

        var context = new ScientistsDbContext(options);

        context.Database.Migrate();

        return context;
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllScientists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        context.Scientists.AddRange(
            new Scientist(
                firstName: "Albert",
                lastName: "Einstein",
                email: "albert@princeton.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Princeton"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Marie",
                lastName: "Curie",
                email: "marie@sorbonne.fr",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Sorbonne"],
                countries: ["France"],
                areas: ["Chemistry"]
            ),
            new Scientist(
                firstName: "Isaac",
                lastName: "Newton",
                email: "isaac@cambridge.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Sir",
                institutions: ["Cambridge"],
                countries: ["UK"],
                areas: ["Physics"]
            )
        );
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: null,
            Country: null,
            Area: null,
            Page: 1,
            PageSize: 10
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Metadata.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task Handle_WithCountryFilter_ReturnsFilteredScientists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        context.Scientists.AddRange(
            new Scientist(
                firstName: "Albert",
                lastName: "Einstein",
                email: "albert@princeton.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Princeton"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Marie",
                lastName: "Curie",
                email: "marie@sorbonne.fr",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Sorbonne"],
                countries: ["France"],
                areas: ["Chemistry"]
            ),
            new Scientist(
                firstName: "Richard",
                lastName: "Feynman",
                email: "richard@caltech.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Caltech"],
                countries: ["USA"],
                areas: ["Physics"]
            )
        );
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: null,
            Country: "USA",
            Area: null,
            Page: 1,
            PageSize: 10
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Metadata.TotalCount);
        Assert.All(result.Items, scientist => Assert.Contains("USA", scientist.Countries));
    }

    [Fact]
    public async Task Handle_WithAreaFilter_ReturnsFilteredScientists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        context.Scientists.AddRange(
            new Scientist(
                firstName: "Albert",
                lastName: "Einstein",
                email: "albert@princeton.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Princeton"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Marie",
                lastName: "Curie",
                email: "marie@sorbonne.fr",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Sorbonne"],
                countries: ["France"],
                areas: ["Chemistry"]
            ),
            new Scientist(
                firstName: "Isaac",
                lastName: "Newton",
                email: "isaac@cambridge.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Sir",
                institutions: ["Cambridge"],
                countries: ["UK"],
                areas: ["Physics"]
            )
        );
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: null,
            Country: null,
            Area: "Physics",
            Page: 1,
            PageSize: 10
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Metadata.TotalCount);
        Assert.All(result.Items, scientist => Assert.Contains("Physics", scientist.Areas));
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ReturnsMatchingScientists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        context.Scientists.AddRange(
            new Scientist(
                firstName: "Albert",
                lastName: "Einstein",
                email: "albert@princeton.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Princeton"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Marie",
                lastName: "Curie",
                email: "marie@sorbonne.fr",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Sorbonne"],
                countries: ["France"],
                areas: ["Chemistry"]
            ),
            new Scientist(
                firstName: "Isaac",
                lastName: "Newton",
                email: "isaac@cambridge.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Sir",
                institutions: ["Cambridge"],
                countries: ["UK"],
                areas: ["Physics"]
            )
        );
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: "einstein",
            Country: null,
            Area: null,
            Page: 1,
            PageSize: 10
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Metadata.TotalCount);
        Assert.Contains(result.Items, s => s.LastName == "Einstein");
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsCorrectlyFilteredScientists()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        context.Scientists.AddRange(
            new Scientist(
                firstName: "Albert",
                lastName: "Einstein",
                email: "albert@princeton.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Princeton"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Richard",
                lastName: "Feynman",
                email: "richard@caltech.edu",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Caltech"],
                countries: ["USA"],
                areas: ["Physics"]
            ),
            new Scientist(
                firstName: "Marie",
                lastName: "Curie",
                email: "marie@sorbonne.fr",
                phoneNumber: null,
                description: null,
                academicTitle: "Dr.",
                institutions: ["Sorbonne"],
                countries: ["France"],
                areas: ["Chemistry"]
            )
        );
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: null,
            Country: "USA",
            Area: "Physics",
            Page: 1,
            PageSize: 10
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Metadata.TotalCount);
        Assert.All(
            result.Items,
            s =>
            {
                Assert.Contains("USA", s.Countries);
                Assert.Contains("Physics", s.Areas);
            }
        );
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        for (int i = 1; i <= 15; i++)
        {
            context.Scientists.Add(
                new Scientist(
                    firstName: $"Scientist{i}",
                    lastName: "Test",
                    email: $"scientist{i}@university.edu",
                    phoneNumber: null,
                    description: null,
                    academicTitle: "Dr.",
                    institutions: ["University"],
                    countries: ["Country"],
                    areas: ["Science"]
                )
            );
        }
        await context.SaveChangesAsync();

        var handler = new GetAllScientistsQueryHandler(context);
        var request = new GetAllScientistsRequest(
            Search: null,
            Country: null,
            Area: null,
            Page: 2,
            PageSize: 5
        );
        var query = new GetAllScientistsQuery(request);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(15, result.Metadata.TotalCount);
        Assert.Equal(5, result.Items.Count());
        Assert.Equal(2, result.Metadata.CurrentPage);
    }
}
