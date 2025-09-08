using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Domain.Entities;

[Table("WalletEntries")]
public class WalletEntry : BaseEntity<Guid>
{
    [Key]
    [Column("Id")]
    public Guid Id { get; set; }

    [Required]
    [Column("WalletId")]
    public Guid WalletId { get; set; }
    
    [Column("BookingId")]
    public Guid? BookingId { get; set; }

    [Required]
    [Column("Amount", TypeName ="decimal(18,2)")]
    [Comment("Can be negative or positive. Negative for going out of the wallet, positive for going in of the wallet.")]
    public decimal Amount { get; set; }

    [Required]
    [Column("Type", TypeName = "varchar(20)")]
    public WalletEntryType Type { get; set; }
    
    [MaxLength(255)]
    public string? Note { get; set; }

    // Navigation property
    [ForeignKey("WalletId")]
    public virtual Wallet Wallet { get; set; } = null!;
    
    // Navigation property
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; } = null!;
}

public enum WalletEntryType
{
    TopUp = 1,
    Withdraw = 2,
    PayDeposit = 3,
    OffSetFinalPayment = 4,
    ReceiveDeposit = 5,
    ReceivePayment = 6,
    ReturnDeposit = 7
}