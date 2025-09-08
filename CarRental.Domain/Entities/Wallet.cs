using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRental.Domain.Entities
{
    [Table("Wallets")]
    public class Wallet : BaseEntity<Guid>
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        public virtual ICollection<WalletEntry> Entries { get; set; } = new List<WalletEntry>();
    }
}
