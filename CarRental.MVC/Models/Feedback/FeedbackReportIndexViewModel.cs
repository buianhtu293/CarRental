using CarRental.Application.DTOs.Feedback;

namespace CarRental.MVC.Models.Feedback
{
    /// <summary>
    /// View model for Feedback Report Index and partial table.
    /// </summary>
    public sealed class FeedbackReportIndexViewModel
    {
        // Filters
        public Guid? CarId { get; set; }
        public int? MinStars { get; set; }
        public int? MaxStars { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Keyword { get; set; }

        // Data
        public List<FeedbackReportItemDto> Items { get; set; } = new();
        public decimal? AverageStars { get; set; }
        public int Count1Star { get; set; }
        public int Count2Stars { get; set; }
        public int Count3Stars { get; set; }
        public int Count4Stars { get; set; }
        public int Count5Stars { get; set; }

        // Cars for dropdown (tuple: id,label)
        public List<(Guid id, string label)> Cars { get; set; } = new();

        // Paging (re-uses your shared paging model)
        public ShopNow.Presentation.Models.PagingModel Paging { get; set; } = new();
    }
}