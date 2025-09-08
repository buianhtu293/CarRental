using CarRental.Domain.Entities;

namespace CarRental.Shared.Utils;

public static class WalletEntryHelper
{
    public static string FromEnumToString(this WalletEntryType type)
    {
        return type switch
        {
            WalletEntryType.TopUp => "Top-up",
            _ => StringFormattingUtils.InsertSpacesInCamelCase(type.ToString())
        };
    }
}