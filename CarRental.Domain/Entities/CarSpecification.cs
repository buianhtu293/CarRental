using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class CarSpecification : BaseEntity<Guid>
    {
        [Required]
        public Guid CarId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool Required { get; set; }

        // Navigation property
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; } = null!;
    }
}
