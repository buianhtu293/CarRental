using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarDetailsDto
    {
        public CarHeaderDto Header { get; set; } = new();
        public CarBasicInfoDto Basic { get; set; } = new();
        public CarDetailInfoDto Details { get; set; } = new();
        public CarTermsDto Terms { get; set; } = new();
    }
}
