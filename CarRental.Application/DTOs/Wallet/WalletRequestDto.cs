namespace CarRental.Application.DTOs.Wallet;

public class WalletRequestDto : PagedRequestDto
{
    public Guid UserId { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}