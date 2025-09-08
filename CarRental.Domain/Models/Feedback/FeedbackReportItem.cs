namespace CarRental.Domain.Models.Feedback
{
    /// <summary>
    /// Read-model representing a single feedback row in reports.
    /// Kept in Domain to avoid cross-layer dependencies.
    /// </summary>
    public sealed class FeedbackReportItem
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