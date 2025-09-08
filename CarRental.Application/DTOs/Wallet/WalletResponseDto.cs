namespace CarRental.Application.DTOs.Wallet;

public class WalletResponseDto : PagedResultDto<WalletEntryResponseDto>
{
    public decimal Balance { get; set; }
    public WalletResponseDto(IEnumerable<WalletEntryResponseDto> items, int totalCount, int page, int pageSize) : base(items, totalCount, page, pageSize)
    {
    }
}