using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class Feedback : BaseEntity<Guid>
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public Guid CarID { get; set; }

        [Required]
        public Guid BookingID { get; set; }

        [Range(1, 5)]
        public int? Stars { get; set; }

        public string? Comment { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("CarID")]
        public virtual Car Car { get; set; } = null!;

        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;
    }
}
