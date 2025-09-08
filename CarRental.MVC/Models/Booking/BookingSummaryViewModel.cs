namespace CarRental.MVC.Models.Booking
{
    public class BookingSummaryViewModel
    {
        public decimal NumberOfDays { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int NumberOfCars { get; set; }
        public List<CarSummaryItem> CarItems { get; set; } = new();
    }
}