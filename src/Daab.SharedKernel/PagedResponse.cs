using Microsoft.EntityFrameworkCore;

namespace Daab.SharedKernel;

/// <summary>
/// Represents a paginated response with metadata and navigation links.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PageMetadata Metadata { get; }

    public PagedResponse(IReadOnlyList<T> items, PageMetadata metadata)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <summary>
    /// Creates a paged response from a full collection.
    /// </summary>
    /// <param name="allItems">The complete collection of items to paginate.</param>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paged response containing the requested page of items.</returns>
    public static PagedResponse<T> Create(IEnumerable<T> allItems, int pageNumber, int pageSize)
    {
        var itemsList = allItems.ToList();
        var totalCount = itemsList.Count;

        var items = itemsList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var metadata = new PageMetadata(
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return new PagedResponse<T>(items, metadata);
    }

    /// <summary>
    /// Creates a paged response when total count is known separately (for database queries).
    /// </summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <returns>A paged response containing the items and pagination metadata.</returns>
    public static PagedResponse<T> Create(
        IReadOnlyList<T> items,
        int pageNumber,
        int pageSize,
        int totalCount
    )
    {
        var metadata = new PageMetadata(
            currentPage: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );

        return new PagedResponse<T>(items, metadata);
    }
}

/// <summary>
/// Contains pagination metadata and navigation information.
/// </summary>
public class PageMetadata
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; }

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; }

    /// <summary>
    /// The index of the first item on the current page (1-based).
    /// </summary>
    public int FirstItemIndex { get; }

    /// <summary>
    /// The index of the last item on the current page (1-based).
    /// </summary>
    public int LastItemIndex { get; }

    public PageMetadata(int currentPage, int pageSize, int totalCount)
    {
        if (currentPage < 1)
        {
            throw new ArgumentException("Page number must be greater than 0.", nameof(currentPage));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
        }

        if (totalCount < 0)
        {
            throw new ArgumentException("Total count cannot be negative.", nameof(totalCount));
        }

        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = currentPage > 1;
        HasNextPage = currentPage < TotalPages;

        if (totalCount == 0)
        {
            FirstItemIndex = 0;
            LastItemIndex = 0;
        }
        else
        {
            FirstItemIndex = ((currentPage - 1) * pageSize) + 1;
            LastItemIndex = Math.Min(currentPage * pageSize, totalCount);
        }
    }
}

/// <summary>
/// Request parameters for pagination.
/// </summary>
public class PageRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>
    /// Page number (1-based). Defaults to 1.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page. Defaults to 20, max 100.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set =>
            _pageSize = value switch
            {
                < 1 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => value,
            };
    }

    /// <summary>
    /// Number of items to skip.
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Number of items to take.
    /// </summary>
    public int Take => PageSize;
}

/// <summary>
/// Extension methods for pagination.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Applies pagination to an IQueryable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="pageRequest">The pagination parameters.</param>
    /// <returns>A queryable with pagination applied.</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PageRequest pageRequest)
    {
        return query.Skip(pageRequest.Skip).Take(pageRequest.Take);
    }

    /// <summary>
    /// Creates a paged response from an IQueryable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the query.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="pageRequest">The pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous operation, containing the paged response.</returns>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PageRequest pageRequest,
        CancellationToken cancellationToken = default
    )
    {
        var totalCount = query.Count();
        var items = await query
            .Skip(pageRequest.Skip)
            .Take(pageRequest.Take)
            .ToListAsync(cancellationToken);

        return PagedResponse<T>.Create(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            totalCount
        );
    }

    public static async Task<PagedResponse<T>> ToPagedResponse<T>(
        this IEnumerable<T> query,
        PageRequest pageRequest
    )
    {
        var totalCount = query.Count();
        var items = query.Skip(pageRequest.Skip).Take(pageRequest.Take).ToList();

        return PagedResponse<T>.Create(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            totalCount
        );
    }
}