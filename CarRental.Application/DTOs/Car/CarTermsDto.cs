using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarTermsDto
    {
        public decimal? BasePricePerDay { get; set; }  
        public decimal? RequiredDeposit { get; set; }   
        //public bool? NoSmoking { get; set; }  
        //public bool? NoPets { get; set; }  
        //public bool? NoFood { get; set; }  
        //public string? Other { get; set; }  
        public string Status { get; set; } = "Available";
    }
}
