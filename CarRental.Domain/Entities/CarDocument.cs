using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class CarDocument : BaseEntity<Guid>
    {
        [Required]
        public Guid CarID { get; set; }

        [MaxLength(50)]
        public string? DocumentType { get; set; }

        [MaxLength(255)]
        public string? FilePath { get; set; }

        // Navigation properties
        [ForeignKey("CarID")]
        public virtual Car Car { get; set; } = null!;
    }
}
