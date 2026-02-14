using Daab.SharedKernel;
using LanguageExt;
using MediatR;

namespace Daab.Modules.Activities.Features.News.GetAllNews;

public class GetAllNewsQuery : IRequest<Fin<PagedResponse<GetAllNewsResponse>>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}