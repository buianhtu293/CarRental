namespace CarRental.Application.DTOs.Feedback
{
    /// <summary>
    /// Represents a request for a feedback report, including filters and paging.
    /// </summary>
    public sealed class FeedbackReportRequestDto
    {
        public Guid? CarId { get; set; }
        public int? MinStars { get; set; }
        public int? MaxStars { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}