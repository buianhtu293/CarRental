namespace CarRental.Application.DTOs;

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResultDto(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items?.ToList().AsReadOnly() ?? new List<T>().AsReadOnly();
        TotalCount = Math.Max(0, totalCount);
        Page = Math.Max(1, page);
        PageSize = Math.Max(1, pageSize);
    }
}