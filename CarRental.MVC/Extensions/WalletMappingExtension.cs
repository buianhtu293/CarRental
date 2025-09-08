using CarRental.Application.DTOs.Wallet;
using CarRental.MVC.Models.Wallet;

namespace CarRental.MVC.Extensions;

public static class WalletMappingExtension
{
    public static WalletIndexViewModel ToViewModel(this WalletResponseDto wallet)
    {
        return new WalletIndexViewModel
        {
            CurrentBalance = wallet.Balance,
            Transactions = wallet.Items,
        };
    }
}