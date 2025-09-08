using CarRental.Application.DTOs;
using CarRental.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.MVC.ViewComponents
{
	public class FeedbackSectionViewComponent : ViewComponent
	{
		private readonly IHomepageService _homepageService;
		public FeedbackSectionViewComponent(IHomepageService homepageService)
		{
			_homepageService = homepageService;
		}	
		private async Task<List<FeedbackDto>> GetTopFeedbackAsync()
		{
			var topFeedbacks = await _homepageService.GetTopFeedbacksAsync();
			return topFeedbacks;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var topFeedbacks = await GetTopFeedbackAsync();
			return View(topFeedbacks);
		}
	}
}
