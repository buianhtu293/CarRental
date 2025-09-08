using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities
{
    public class BookingItem : BaseEntity<Guid>
    {
        [Required]
        public Guid BookingID { get; set; }

        [Required]
        public Guid CarID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PricePerDay { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Deposit { get; set; }

        [Required]
        public string LicenseID { get; set; }
        [MaxLength(255)]
        public string LicenseImage { get; set; }
        [MaxLength(100)]
        public string? FullName { get; set; }
        public DateTime? DOB { get; set; }
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }

        [MaxLength(20)]
        public BookingItemStatusEnum Status { get; set; } = BookingItemStatusEnum.PendingDeposit;

        // Navigation properties
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("CarID")]
        public virtual Car Car { get; set; } = null!;
    }
}
