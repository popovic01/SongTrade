using Microsoft.AspNetCore.Mvc;

namespace SongTrade.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
