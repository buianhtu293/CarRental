using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Domain.Models
{
    public class CarSearchCriteria
    {
        public DateTime? PickupDateTime { get; set; }
        public DateTime? ReturnDateTime { get; set; }
        public string? ProvinceName { get; set; }
        public int? ProvinceId { get; set; }
        public string? DistrictName { get; set; }
        public int? DistrictId { get; set; }
        public string? WardName { get; set; }
        public string? SpecificAddress { get; set; }
        public int? WardId { get; set; }
    }
}
