using System.Security.Claims;
using CarRental.Application.Interfaces;
using CarRental.MVC.Models.Cart;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.MVC.Controllers
{
	public class CartController(ICartService cartService) : Controller
	{
		public async Task<IActionResult> Index()
		{
			var userId = User.Claims
		.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			var cart = await cartService.GetCart(Guid.Parse(userId!));

			var model = new CartViewModel
			{
				CartDTO = cart
			};

			return View(model);
		}

		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest addToCartRequest)
		{
			var userId = User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				return Unauthorized();

			bool result = await cartService.AddToCart(Guid.Parse(userId), addToCartRequest.carId);

			if (result)
				return Ok(new { message = "Car added to cart successfully." });
			else
				return BadRequest(new { message = "Car already existed in cart." });
		}


		[HttpPost]
		public async Task<IActionResult> RemoveCartItem([FromBody] RemoveCartItemRequest request)
		{
			await cartService.RemoveItem(request.ItemId);

			return RedirectToAction(nameof(Index), "Cart");
		}
	}
}
