using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarRental.Application.DTOs;
using CarRental.Application.DTOs.Cart;
using CarRental.Application.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Application.Services
{
	public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<User> _userManager;

		public CartService(IUnitOfWork unitOfWork,
			UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
			_userManager = userManager;
		}

        public async Task<bool> AddToCart(Guid userId, Guid carId)
        {
            var cart = _unitOfWork.Carts
                .GetFirstOrDefaultWithThenIncluding(
                    c => c.UserID == userId,
                    c => c.Include(c => c.CartItems)
                        .ThenInclude(c => c.Car)
                        .ThenInclude(c => c.CarDocuments)
                );

            if (cart == null)
            {
                var cartToCreate = new Cart { UserID = userId };
                await _unitOfWork.Carts.AddAsync(cartToCreate);
                await _unitOfWork.SaveChangesAsync();

                cart = cartToCreate;
                cart.CartItems = new List<CartItem>();
            }

            // Check if car already exists in the cart
            bool alreadyInCart = cart.CartItems.Any(ci => ci.CarID == carId);
            if (alreadyInCart)
                return false;

            // Fetch the car
            var car = await _unitOfWork.Cars
                .GetFirstOrDefaultAsync(x => x.Id == carId);

            if (car == null)
                return false;

            // Create new cart item
            var cartItem = new CartItem
            {
                CartID = cart.Id,
                CarID = car.Id,
                PickupDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow,
                PricePerDay = car.BasePricePerDay,
                Deposit = car.RequiredDeposit
            };

            cart.CartItems.Add(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }


        public async Task<CartDTO> GetCart(Guid userId)
        {
            var cart = _unitOfWork.Carts
                .GetFirstOrDefaultWithThenIncluding(
                    c => c.UserID == userId,
                    c => c.Include(c => c.CartItems)
                        .ThenInclude(c => c.Car)
                        .ThenInclude(c => c.CarDocuments)
                );


            if(cart == null)
            {
                var cartToCreate = new Cart { UserID = userId };
                await _unitOfWork.Carts.AddAsync(cartToCreate);
                await _unitOfWork.SaveChangesAsync();

                cart = cartToCreate; // Or reload from DB if you need related entities
                cart.CartItems = new List<CartItem>();
            }

            var cartDto = new CartDTO
            {
                CartItems = cart.CartItems.Select(ci => new CartItemDTO
                {
                    ID = ci.Id,
                    CartID = ci.CartID,
                    CarID = ci.CarID,
                    PickupDate = ci.PickupDate,
                    ReturnDate = ci.ReturnDate,
                    PricePerDay = ci.PricePerDay,
                    Deposit = ci.Deposit,
                    Car = new CarCartDTO
                    {
                        ID = ci.Car.Id,
                        Brand = ci.Car.Brand,
                        Model = ci.Car.Model,
                        ProductionYear = ci.Car.ProductionYear,
                        CarDocuments = ci.Car.CarDocuments.Select(doc => new CarDocumentCartDTO
                        {
                            ID = doc.Id,
                            DocumentType = doc.DocumentType,
                            FilePath = doc.FilePath
                        }).ToList()
                    }
                }).ToList()
            };

            return cartDto;
        }

		public async Task<int> RemoveItem(Guid itemId)
		{
			await _unitOfWork.CartItems.DeleteAsync(itemId);
            var result = await _unitOfWork.SaveChangesAsync();

            return result;
		}
	}
}
