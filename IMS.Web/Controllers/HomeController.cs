using IMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    /// <summary>
    /// Home controller for dashboard and landing page
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IStockService stockService, ILogger<HomeController> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard with inventory statistics
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _stockService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Error loading dashboard data.";
                return View();
            }
        }

        /// <summary>
        /// Privacy policy page
        /// </summary>
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Error handler
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
