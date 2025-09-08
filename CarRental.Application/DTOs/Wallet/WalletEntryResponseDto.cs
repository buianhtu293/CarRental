using CarRental.Domain.Entities;

namespace CarRental.Application.DTOs.Wallet;

public class WalletEntryResponseDto
{
    public decimal Amount { get; set; }
    public WalletEntryType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BookingNumber { get; set; }
    public string? CarName { get; set; }
    public string? Note { get; set; }
}