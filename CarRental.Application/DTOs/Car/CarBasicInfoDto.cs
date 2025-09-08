using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarBasicInfoDto
    {
        public string LicensePlate { get; set; } = string.Empty; 
        public string? Brand { get; set; }                       
        public string? Model { get; set; }  
        public int? ProductionYear { get; set; }  
        public string? Color { get; set; } 
        public int? Seats { get; set; }     
        public string? Transmission { get; set; } 
        public string? FuelType { get; set; } 

        public IReadOnlyList<CarDocumentDto> Documents { get; set; } = Array.Empty<CarDocumentDto>();
    }
}
