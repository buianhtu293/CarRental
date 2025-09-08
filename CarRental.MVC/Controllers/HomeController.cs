using CarRental.Application.Interfaces;
using CarRental.Application.Services;
using CarRental.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarRental.MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomepageService _homepageService;

    public HomeController(ILogger<HomeController> logger, IHomepageService homepageService)
    {
        _logger = logger;
        _homepageService = homepageService;

    }

    public async Task<IActionResult> Index()
    {
        var topFeedbacksTask = await _homepageService.GetTopFeedbacksAsync();
        var topCitiesTask = await _homepageService.GetTopCitiesAsync();


        var model = new GuestHomepageViewModel
        {
            TopFeedbacks = topFeedbacksTask,
            TopCities = topCitiesTask
        };

        return View(model);

    }

    //[Authorize(Roles = "Owner")]
    public IActionResult Owner()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
