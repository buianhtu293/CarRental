namespace CarRental.Application.DTOs.Booking
{
    /// <summary>
    /// Class dùng để chứa tóm tắt booking, bao gồm số ngày thuê, 
    /// giá mỗi ngày, tổng số tiền, tiền đặt cọc, ngày nhận và trả xe, và danh sách các xe đã chọn.
    /// </summary>
    public class BookingSummaryDto
    {
        public decimal NumberOfDays { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDeposit { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int NumberOfCars { get; set; }
        public List<CarSummaryItemDto> CarItems { get; set; } = new();
    }
}