namespace CarRental.Application.DTOs.Feedback
{
    /// <summary>
    /// A paged feedback report with items and aggregates.
    /// </summary>
    public sealed class FeedbackReportDto
    {
        public List<FeedbackReportItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public decimal? AverageStars { get; set; }
        public int Count1Star { get; set; }
        public int Count2Stars { get; set; }
        public int Count3Stars { get; set; }
        public int Count4Stars { get; set; }
        public int Count5Stars { get; set; }
    }
}