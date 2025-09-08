using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Domain.Entities;

namespace CarRental.Application.DTOs.Cart
{
    public class CartItemDTO
    {
		public Guid ID { get; set; }

		public Guid CartID { get; set; }

        public Guid CarID { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime ReturnDate { get; set; }

        public decimal? PricePerDay { get; set; }

        public decimal? Deposit { get; set; }

        public virtual CarCartDTO Car { get; set; } = null!;
    }
}
