using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarDetailInfoDto
    {
        public int? Mileage { get; set; }            
        public decimal? FuelConsumption { get; set; }
        public string? Address { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Description { get; set; }   
        public IReadOnlyList<CarSpecificationDto> Specifications { get; set; } = Array.Empty<CarSpecificationDto>();
    }
}
