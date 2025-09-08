using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class BrandDto
    {
        public string? BrandName { get; set; } = string.Empty;

        public string? ModelName { get; set; } = string.Empty;
    }
}

