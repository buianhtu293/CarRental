using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Car
{
    public class CarDocumentDto
    {
        public Guid Id { get; set; }
        public string? DocumentType { get; set; }
        public string? FilePath { get; set; }
    }
}
