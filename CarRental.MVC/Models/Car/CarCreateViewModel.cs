using CarRental.Application.DTOs;
using CarRental.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarRental.MVC.Models.Car
{
    public class CarCreateViewModel
    {
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

        //public List<IFormFile> Files { get; set; }

        public virtual ICollection<CarDocumentDto> CarDocuments { get; set; } = new List<CarDocumentDto>();

        public virtual ICollection<CarSpecificationDto> CarSpecifications { get; set; } = new List<CarSpecificationDto>();
    }
}
