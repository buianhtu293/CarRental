using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Application.DTOs.Cart
{
    public class CartDTO
    {
        public IEnumerable<CartItemDTO> CartItems { get; set; } = null!;
    }
}
