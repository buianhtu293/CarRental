using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.MVC.ViewComponents
{
	public class WhereToFindUsViewComponent : ViewComponent
	{
		private readonly IHomepageService _homepageService;
		public WhereToFindUsViewComponent(IHomepageService homepageService)
		{
			_homepageService = homepageService;
		}
		private async Task<List<CitySummaryDto>> GetTopCitiesAsync()
		{
			var cities = await _homepageService.GetTopCitiesAsync();
			return cities;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var topCities = await GetTopCitiesAsync();
			return View(topCities);
		}
	}
}
