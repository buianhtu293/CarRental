using CarRental.Application.DTOs;
using CarRental.Domain.Models;

namespace CarRental.MVC.Models
{
    public class SearchViewModel
    {
        public SearchFormModel SearchCriteria { get; set; }

        public PagedResult<CarSearchDto> SearchResult { get; set; }

        public string? CurrentSort { get; set; }
        public string? CurrentView { get; set; }
    }
}
