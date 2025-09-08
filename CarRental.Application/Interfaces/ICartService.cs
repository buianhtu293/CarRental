using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs.Cart;
using CarRental.Domain.Entities;

namespace CarRental.Application.Interfaces
{
    public interface ICartService
    {
        Task<bool> AddToCart(Guid userId, Guid carId);

        Task<CartDTO> GetCart(Guid userId);

        Task<int> RemoveItem(Guid itemId);
    }
}
