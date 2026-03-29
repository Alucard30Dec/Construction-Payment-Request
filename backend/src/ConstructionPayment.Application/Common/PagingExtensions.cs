using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Application.Common;

public static class PagingExtensions
{
    public static async Task<PagedResult<TOutput>> ToPagedResultAsync<TSource, TOutput>(
        this IQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Func<TSource, TOutput> selector,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = pageNumber <= 0 ? 1 : pageNumber;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TOutput>
        {
            Items = items.Select(selector).ToArray(),
            TotalCount = totalCount,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }

    public static async Task<PagedResult<TOutput>> ToPagedResultProjectedAsync<TSource, TOutput>(
        this IQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Expression<Func<TSource, TOutput>> selector,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = pageNumber <= 0 ? 1 : pageNumber;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPage - 1) * normalizedSize)
            .Take(normalizedSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TOutput>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = normalizedPage,
            PageSize = normalizedSize
        };
    }
}
