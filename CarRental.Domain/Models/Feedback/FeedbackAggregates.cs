namespace CarRental.Domain.Models.Feedback
{
    /// <summary>
    /// Lightweight aggregate read-model returned by repository reporting queries.
    /// </summary>
    public sealed class FeedbackAggregates
    {
        /// <summary>Total number of feedback records matching the filter.</summary>
        public int Total { get; set; }

        /// <summary>Average stars (rounded to two decimals).</summary>
        public decimal AverageStars { get; set; }

        /// <summary>Count of 1-star feedbacks.</summary>
        public int Count1 { get; set; }

        /// <summary>Count of 2-star feedbacks.</summary>
        public int Count2 { get; set; }

        /// <summary>Count of 3-star feedbacks.</summary>
        public int Count3 { get; set; }

        /// <summary>Count of 4-star feedbacks.</summary>
        public int Count4 { get; set; }

        /// <summary>Count of 5-star feedbacks.</summary>
        public int Count5 { get; set; }
    }
}