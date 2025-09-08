using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class CarDocumentDto
    {
        public Guid Id { get; set; }
        public string? DocumentType { get; set; }
        public string? FilePath { get; set; }
        public IFormFile ? File { get; set; }
    }
}
