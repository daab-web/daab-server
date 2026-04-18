using System.Diagnostics.CodeAnalysis;
using Daab.Modules.Activities.Features.News.GetTranslations;
using Daab.Modules.Activities.Models;
using Daab.Modules.Activities.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Activities.Tests;

public sealed class GetNewsTranslationsTests : IDisposable, IAsyncLifetime
{
    private ActivitiesDbContext _ctx = null!;

    [Fact]
    public async Task GetAllTranslations_NoFilter_ReturnsArrayWithThreeEntries()
    {
        // Arrange
        var cmd = new GetTranslationsQuery(null);
        var handler = new GetTranslationsQueryHandler(_ctx);

        // Act
        var res = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.Equal(3, res.Length);
        Assert.Contains("en", res.Select(t => t.Locale));
        Assert.Contains("ru", res.Select(t => t.Locale));
        Assert.Contains("az", res.Select(t => t.Locale));
    }

    [Fact]
    public async Task GetAllTranslations_WithFilter_ReturnsOnlyTranslatedEntries()
    {
        // Arrange
        var cmd = new GetTranslationsQuery(filter: "translated");
        var handler = new GetTranslationsQueryHandler(_ctx);

        // Act
        var res = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.Empty(res);
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    public async Task InitializeAsync()
    {
        var opts = new DbContextOptionsBuilder<ActivitiesDbContext>()
            .UseInMemoryDatabase(Ulid.NewUlid().ToString())
            .Options;

        _ctx = new ActivitiesDbContext(opts);

        var news = new News
        {
            Title = "Test title",
            Slug = "test-title",
            PublishedDate = DateTime.UtcNow,
        };
        news.Translations = new List<NewsTranslation>
        {
            new() { NewsId = news.Id, Locale = "en" },
            new() { NewsId = news.Id, Locale = "ru" },
            new() { NewsId = news.Id, Locale = "az" },
        };

        _ctx.News.Add(news);
        await _ctx.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }
}
