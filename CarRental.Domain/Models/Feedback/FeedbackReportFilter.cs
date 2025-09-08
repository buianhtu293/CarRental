namespace CarRental.Domain.Models.Feedback
{
    /// <summary>
    /// Filter criteria for feedback reporting.
    /// This type lives in the Domain project so repositories can expose reporting methods
    /// without depending on Application-layer DTOs.
    /// </summary>
    public sealed class FeedbackReportFilter
    {
        /// <summary>Filter by Car Id (optional).</summary>
        public Guid? CarId { get; set; }

        /// <summary>Exact stars to filter (1-5) — optional.</summary>
        public int? Stars { get; set; }

        /// <summary>Minimum stars to filter (optional).</summary>
        public int? MinStars { get; set; }

        /// <summary>Maximum stars to filter (optional).</summary>
        public int? MaxStars { get; set; }

        /// <summary>From date for CreatedAt filter (optional).</summary>
        public DateTime? FromDate { get; set; }

        /// <summary>To date for CreatedAt filter (optional).</summary>
        public DateTime? ToDate { get; set; }

        /// <summary>Keyword to search in comment or username.</summary>
        public string? Keyword { get; set; }
    }
}