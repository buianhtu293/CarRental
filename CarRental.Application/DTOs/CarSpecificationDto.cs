using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class CarSpecificationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool Required { get; set; }
    }
}
