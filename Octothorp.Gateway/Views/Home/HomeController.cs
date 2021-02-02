using Microsoft.AspNetCore.Mvc;

namespace Octothorp.Gateway.Views
{
    public class HomeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}