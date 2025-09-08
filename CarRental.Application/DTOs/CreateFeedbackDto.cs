using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class CreateFeedbackDto
    {
        public Guid UserID { get; set; }
        public Guid CarID { get; set; }
        public Guid BookingID { get; set; }
        public int Stars { get; set; }
        public string? Comment { get; set; }
    }
}
