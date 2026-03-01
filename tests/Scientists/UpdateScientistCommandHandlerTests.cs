using Daab.Modules.Scientists.Features.Scientists.UpdateScientist;
using Daab.Modules.Scientists.Models;
using Daab.Modules.Scientists.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Scientists.Tests;

public class UpdateScientistCommandHandlerTests
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
    public async Task Handle_WithValidId_UpdatesScientistSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var scientist = new Scientist(
            firstName: "Albert",
            lastName: "Einstein",
            email: "albert@princeton.edu",
            phoneNumber: "+1234567890",
            description: "Theoretical physicist",
            academicTitle: "Dr.",
            institutions: ["Princeton"],
            countries: ["USA"],
            areas: ["Physics"]
        );

        context.Scientists.Add(scientist);
        await context.SaveChangesAsync();

        var handler = new UpdateScientistCommandHandler(context);
        var request = new UpdateScientistRequest(
            Email: "albert.updated@princeton.edu",
            PhoneNumber: "+9876543210",
            FirstName: "Albert Updated",
            LastName: "Einstein Updated",
            Description: "Updated description",
            AcademicTitle: "Prof.",
            Institution: ["Princeton University"],
            Countries: ["USA", "Germany"],
            Areas: ["Physics", "Mathematics"]
        );
        var command = new UpdateScientistCommand(scientist.Id, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSucc);
        result.Match(
            Succ: response => Assert.Equal(scientist.Id, response.Id),
            Fail: error => Assert.Fail($"Expected success but got error: {error.Message}")
        );

        // Verify changes were persisted
        var updatedScientist = await context.Scientists.FindAsync(scientist.Id);
        Assert.NotNull(updatedScientist);
        Assert.Equal("Albert Updated", updatedScientist.FirstName);
        Assert.Equal("Einstein Updated", updatedScientist.LastName);
        Assert.Equal("albert.updated@princeton.edu", updatedScientist.Email);
        Assert.Equal("+9876543210", updatedScientist.PhoneNumber);
        Assert.Equal("Updated description", updatedScientist.Description);
        Assert.Equal("Prof.", updatedScientist.AcademicTitle);
        Assert.Equal(["Princeton University"], updatedScientist.Institutions);
        Assert.Equal(["USA", "Germany"], updatedScientist.Countries);
        Assert.Equal(["Physics", "Mathematics"], updatedScientist.Areas);
    }

    [Fact]
    public async Task Handle_WithNullableFields_UpdatesScientistSuccessfully()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var scientist = new Scientist(
            firstName: "Marie",
            lastName: "Curie",
            email: "marie@sorbonne.fr",
            phoneNumber: "+1234567890",
            description: "Physicist and chemist",
            academicTitle: "Dr.",
            institutions: ["Sorbonne"],
            countries: ["France"],
            areas: ["Chemistry"]
        );

        context.Scientists.Add(scientist);
        await context.SaveChangesAsync();

        var handler = new UpdateScientistCommandHandler(context);
        var request = new UpdateScientistRequest(
            Email: "marie@sorbonne.fr",
            PhoneNumber: null,
            FirstName: "Marie",
            LastName: "Curie",
            Description: null,
            AcademicTitle: "Dr.",
            Institution: ["Sorbonne"],
            Countries: ["France"],
            Areas: ["Chemistry", "Physics"]
        );
        var command = new UpdateScientistCommand(scientist.Id, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSucc);

        var updatedScientist = await context.Scientists.FindAsync(scientist.Id);
        Assert.NotNull(updatedScientist);
        Assert.Null(updatedScientist.PhoneNumber);
        Assert.Null(updatedScientist.Description);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsError()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var handler = new UpdateScientistCommandHandler(context);
        var nonExistentId = Ulid.NewUlid().ToString();
        var request = new UpdateScientistRequest(
            Email: "test@university.edu",
            PhoneNumber: null,
            FirstName: "Test",
            LastName: "Scientist",
            Description: null,
            AcademicTitle: "Dr.",
            Institution: ["University"],
            Countries: ["Country"],
            Areas: ["Science"]
        );
        var command = new UpdateScientistCommand(nonExistentId, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFail);
        result.Match(
            Succ: _ => Assert.Fail("Expected failure but got success"),
            Fail: error =>
                Assert.Contains($"Scientist with an Id of {nonExistentId} not found", error.Message)
        );
    }

    [Fact]
    public async Task Handle_UpdatesOnlySpecifiedScientist()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var scientist1 = new Scientist(
            firstName: "Albert",
            lastName: "Einstein",
            email: "albert@princeton.edu",
            phoneNumber: null,
            description: null,
            academicTitle: "Dr.",
            institutions: ["Princeton"],
            countries: ["USA"],
            areas: ["Physics"]
        );

        var scientist2 = new Scientist(
            firstName: "Isaac",
            lastName: "Newton",
            email: "isaac@cambridge.edu",
            phoneNumber: null,
            description: null,
            academicTitle: "Sir",
            institutions: ["Cambridge"],
            countries: ["UK"],
            areas: ["Physics"]
        );

        context.Scientists.AddRange(scientist1, scientist2);
        await context.SaveChangesAsync();

        var handler = new UpdateScientistCommandHandler(context);
        var request = new UpdateScientistRequest(
            Email: "albert.updated@princeton.edu",
            PhoneNumber: null,
            FirstName: "Albert Updated",
            LastName: "Einstein Updated",
            Description: null,
            AcademicTitle: "Prof.",
            Institution: ["Princeton"],
            Countries: ["USA"],
            Areas: ["Physics"]
        );
        var command = new UpdateScientistCommand(scientist1.Id, request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSucc);

        // Verify only scientist1 was updated
        var updatedScientist1 = await context.Scientists.FindAsync(scientist1.Id);
        Assert.Equal("Albert Updated", updatedScientist1!.FirstName);
        Assert.Equal("Einstein Updated", updatedScientist1.LastName);

        // Verify scientist2 was not modified
        var unchangedScientist2 = await context.Scientists.FindAsync(scientist2.Id);
        Assert.Equal("Isaac", unchangedScientist2!.FirstName);
        Assert.Equal("Newton", unchangedScientist2.LastName);
        Assert.Equal("isaac@cambridge.edu", unchangedScientist2.Email);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        await using var context = CreateInMemoryContext();

        var scientist = new Scientist(
            firstName: "Test",
            lastName: "Scientist",
            email: "test@university.edu",
            phoneNumber: null,
            description: null,
            academicTitle: "Dr.",
            institutions: ["University"],
            countries: ["Country"],
            areas: ["Science"]
        );

        context.Scientists.Add(scientist);
        await context.SaveChangesAsync();

        var handler = new UpdateScientistCommandHandler(context);
        var request = new UpdateScientistRequest(
            Email: "updated@university.edu",
            PhoneNumber: null,
            FirstName: "Updated",
            LastName: "Name",
            Description: null,
            AcademicTitle: "Dr.",
            Institution: ["University"],
            Countries: ["Country"],
            Areas: ["Science"]
        );
        var command = new UpdateScientistCommand(scientist.Id, request);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await handler.Handle(command, cts.Token)
        );
    }
}
