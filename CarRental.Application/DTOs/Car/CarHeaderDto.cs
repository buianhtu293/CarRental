using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarHeaderDto
    {
        public Guid CarId { get; set; }
        public List<string> ImageUrls { get; set; }
        public string DisplayName { get; set; } = string.Empty; 
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string? LocationText { get; set; } 
        public bool IsBookedByCurrentUser { get; set; }
    }
}
