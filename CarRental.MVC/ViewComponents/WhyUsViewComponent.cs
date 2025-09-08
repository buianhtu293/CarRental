using Microsoft.AspNetCore.Mvc;
namespace CarRental.MVC.ViewComponents
{
	public class WhyUsViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}
