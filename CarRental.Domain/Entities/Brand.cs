using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Entities
{
    public class Brand : BaseEntity<Guid>
    {
        [Required]
        [MaxLength(100)]
        public string? BrandName { get; set; } = string.Empty;


        [Required]
        [MaxLength(100)]
        public string? ModelName { get; set; } = string.Empty;

    }
}
