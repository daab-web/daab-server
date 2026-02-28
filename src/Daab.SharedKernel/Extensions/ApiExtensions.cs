using System.Diagnostics;
using FastEndpoints;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;

namespace Daab.SharedKernel.Extensions;

public static class ApiExtensions
{
    extension(Error error)
    {
        public ProblemDetails ToProblemDetails(HttpContext httpContext)
        {
            return new ProblemDetails
            {
                Detail = error.Message,
                Status = error.Code,
                Instance = httpContext.Request.Path,
                TraceId = Activity.Current?.Id ?? httpContext.TraceIdentifier,
            };
        }
    }
}
