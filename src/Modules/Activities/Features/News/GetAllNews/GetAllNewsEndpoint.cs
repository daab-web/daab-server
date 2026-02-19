using Daab.SharedKernel;
using Daab.SharedKernel.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Activities.Features.News.GetAllNews;

public class GetAllNewsEndpoint(IMediator mediator)
    : Endpoint<GetAllNewsRequest, PagedResponse<GetAllNewsResponse>>
{
    public override void Configure()
    {
        Get("/news");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAllNewsRequest request, CancellationToken ct)
    {
        var res = await mediator.Send(
            new GetAllNewsQuery { Page = request.Page, PageSize = request.PageSize },
            ct
        );

        await res.Match(
            news => Send.OkAsync(news, ct),
            err => Send.ResultAsync(TypedResults.Problem(err.ToProblemDetails(HttpContext)))
        );
    }
}
