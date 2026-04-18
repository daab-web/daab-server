using Daab.Modules.Activities.Features.News.CreateNews;
using Daab.Modules.Activities.Persistence;
using Daab.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Tests;

public sealed class CreateNewsTests : IAsyncLifetime, IDisposable
{
    private ActivitiesDbContext _ctx = null!;

    [Fact]
    public async Task CreateNews_WithValidData_CreatesNewsAndEmptyTranslations()
    {
        // Arrange
        var req = new CreateNewsRequest(
            "Test title",
            "",
            null,
            "Test excerpt",
            null,
            "Test Category",
            ["test-tag"],
            DateTime.UtcNow
        );
        var cmd = new CreateNewsCommand(req);
        var handler = new CreateNewsCommandHandler(_ctx);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        var response = result.Match(
            x => x,
            err =>
            {
                Assert.Fail(err.Message);
                return null;
            }
        );
        Assert.NotNull(_ctx.News.SingleOrDefault(n => n.Id == response.Id));
        Assert.Equal(
            Localization.SupportedLocales.Length,
            _ctx.NewsTranslations.Count(t => t.NewsId == response.Id)
        );
    }

    public Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<ActivitiesDbContext>()
            .UseInMemoryDatabase("activities_test_db")
            .Options;

        _ctx = new ActivitiesDbContext(opts);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }
}
