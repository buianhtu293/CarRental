using CarRental.Application.DTOs;

namespace CarRental.MVC.Models
{
    public class GuestHomepageViewModel
    {
        public List<FeedbackDto> TopFeedbacks { get; set; }
        public List<CitySummaryDto> TopCities { get; set; }
        public SearchFormModel SearchFormModel { get; set; }
    }
}
