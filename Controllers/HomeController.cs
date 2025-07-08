using Microsoft.AspNetCore.Mvc;

namespace Gapsi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

