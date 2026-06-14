using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.UpdateNews;

public sealed class UpdateNewsCommand : IRequest<Fin<UpdateNewsResponse>>
{
    [SetsRequiredMembers]
    public UpdateNewsCommand(string id, UpdateNewsRequest request)
    {
        Id = id;
        Category = request.Category;
        Tags = request.Tags;
        PublishedDate = request.PublishedDate;
    }

    public required string Id { get; init; }
    public string? Category { get; init; }
    public List<string> Tags { get; init; }
    public DateTime PublishedDate { get; init; }
}
