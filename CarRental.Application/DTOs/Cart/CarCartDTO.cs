using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;

namespace CarRental.Application.DTOs.Cart
{
    public class CarCartDTO
    {
        public Guid ID { get; set; }
        public Guid OwnerID { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }

        public int? ProductionYear { get; set; }
        public string? Color { get; set; }

        public int? Seats { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }

        public int? Mileage { get; set; }
        public decimal? FuelConsumption { get; set; }

        public string? Description { get; set; }

        public decimal? BasePricePerDay { get; set; }
        public decimal? RequiredDeposit { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? District { get; set; }

        public string? Ward { get; set; }
        public string Status { get; set; } = "Available";

        public virtual ICollection<CarDocumentCartDTO> CarDocuments { get; set; } = new List<CarDocumentCartDTO>();
    }
}
