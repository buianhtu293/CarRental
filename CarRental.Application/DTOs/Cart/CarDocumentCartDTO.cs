using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Cart
{
    public class CarDocumentCartDTO
    {
        public Guid ID { get; set; }
        public Guid CarID { get; set; }

        public string? DocumentType { get; set; }

        public string? FilePath { get; set; }
    }
}
