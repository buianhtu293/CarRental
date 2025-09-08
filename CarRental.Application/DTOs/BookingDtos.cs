namespace CarRental.Application.DTOs
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? PickupTime { get; set; }
        public DateTime? ReturnTime { get; set; }
        public UserDto? User { get; set; }
        public CarDto? Car { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBookingDto
    {
        public int UserId { get; set; }
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}