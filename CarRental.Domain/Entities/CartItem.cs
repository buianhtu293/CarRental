using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class CartItem : BaseEntity<Guid>
    {
        [Required]
        public Guid CartID { get; set; }

        [Required]
        public Guid CarID { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PricePerDay { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Deposit { get; set; }

        // Navigation properties
        [ForeignKey("CartID")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("CarID")]
        public virtual Car Car { get; set; } = null!;
    }
}
