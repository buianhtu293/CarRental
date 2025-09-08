using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Domain.Entities
{
    public class Booking : BaseEntity<Guid>
    {
        [MaxLength(20)]
        public string? BookingNo { get; set; }

        [Required]
        public Guid RenterID { get; set; }

        public DateTime? PickupDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        [MaxLength(20)]
        public string? TransactionType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }
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

        // Navigation properties
        [ForeignKey("RenterID")]
        public virtual User Renter { get; set; } = null!;
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
