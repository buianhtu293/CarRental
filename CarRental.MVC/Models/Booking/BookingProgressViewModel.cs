namespace CarRental.MVC.Models.Booking
{
    public class BookingProgressViewModel
    {
        public int CurrentStep { get; set; } = 1;
        public int TotalSteps { get; set; } = 3;
        public string BookingSessionId { get; set; } = string.Empty;
        
        public bool IsStep1Active => CurrentStep == 1;
        public bool IsStep1Completed => CurrentStep > 1;
        
        public bool IsStep2Active => CurrentStep == 2;
        public bool IsStep2Completed => CurrentStep > 2;
        
        public bool IsStep3Active => CurrentStep == 3;
        public bool IsStep3Completed => CurrentStep > 3;
    }
}