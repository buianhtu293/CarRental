using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class Car : BaseEntity<Guid>
    {
        [Required]
        public Guid OwnerID { get; set; }

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Brand { get; set; }

        [MaxLength(50)]
        public string? Model { get; set; }

        public int? ProductionYear { get; set; }

        [MaxLength(30)]
        public string? Color { get; set; }

        public int? Seats { get; set; }

        [MaxLength(10)]
        public string? Transmission { get; set; }

        [MaxLength(10)]
        public string? FuelType { get; set; }

        public int? Mileage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FuelConsumption { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? BasePricePerDay { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? RequiredDeposit { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? District { get; set; }

        public string? Ward { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Available";

        // Navigation properties
        [ForeignKey("OwnerID")]
        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<CarDocument> CarDocuments { get; set; } = new List<CarDocument>();
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<CarSpecification> CarSpecifications { get; set; } = new List<CarSpecification>();
    }
}
