using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs
{
    public class FeedbackDto
    {
        public string UserName { get; set; }
        public string Comment { get; set; }
        public int? Rating { get; set; }
        public DateTime? FeedbackDate { get; set; }
    }
}
