namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// DTO to track booking progress through steps: Information ? Payment ? Confirmation.
    /// Provides information about current step and completion status of each step.
    /// </summary>
    public class BookingProgressDto
    {
        /// <summary>
        /// Current step in booking process (1: Information, 2: Payment, 3: Confirmation)
        /// </summary>
        public int CurrentStep { get; set; } = 1;
        
        /// <summary>
        /// Total number of steps in booking process
        /// </summary>
        public int TotalSteps { get; set; } = 3;
        
        /// <summary>
        /// Booking session code to track process
        /// </summary>
        public string BookingSessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether step 1 (Information) is currently active
        /// </summary>
        public bool IsStep1Active => CurrentStep == 1;
        
        /// <summary>
        /// Indicates whether step 1 (Information) is completed
        /// </summary>
        public bool IsStep1Completed => CurrentStep > 1;
        
        /// <summary>
        /// Indicates whether step 2 (Payment) is currently active
        /// </summary>
        public bool IsStep2Active => CurrentStep == 2;
        
        /// <summary>
        /// Indicates whether step 2 (Payment) is completed
        /// </summary>
        public bool IsStep2Completed => CurrentStep > 2;
        
        /// <summary>
        /// Indicates whether step 3 (Confirmation) is currently active
        /// </summary>
        public bool IsStep3Active => CurrentStep == 3;
        
        /// <summary>
        /// Indicates whether step 3 (Confirmation) is completed
        /// </summary>
        public bool IsStep3Completed => CurrentStep > 3;
    }
}