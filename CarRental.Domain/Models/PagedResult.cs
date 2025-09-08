using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items?.ToList().AsReadOnly() ?? new List<T>().AsReadOnly();
            TotalCount = Math.Max(0, totalCount);
            Page = Math.Max(1, page);
            PageSize = Math.Max(1, pageSize);
        }
    }
}
