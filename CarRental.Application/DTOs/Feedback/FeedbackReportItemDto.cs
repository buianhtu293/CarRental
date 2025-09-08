namespace CarRental.Application.DTOs.Feedback
{
    /// <summary>
    /// A single feedback record in the feedback report.
    /// </summary>
    public sealed class FeedbackReportItemDto
    {
        public Guid FeedbackId { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
        public string? UserName { get; set; }
        public Guid CarId { get; set; }
        public string? CarLabel { get; set; }
        public string? BookingNo { get; set; }
        public Guid BookingItemId { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public List<string>? CarImageUrls { get; set; }
    }
}