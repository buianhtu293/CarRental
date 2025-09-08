namespace CarRental.MVC.Models.CarOwnerBooking;

public sealed class CarOwnerBookingItemViewModel
{
    public string CarName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;

    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public decimal SubTotal { get; set; }
}