using Microsoft.AspNetCore.Mvc;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;
using System.Globalization;
using System.Web.WebPages;

namespace SongTrade.Controllers
{
    [AuthRole("Role", "buyer")]
    public class OrderController : Controller
    {
        
        private readonly IOrderHeaderRepository _orderRepo;

        public OrderController(IOrderHeaderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public IActionResult Index()
        {
            return View();  
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            int userId = int.Parse(HttpContext.Session.GetString(StaticDetails.UserId));
            IEnumerable<OrderHeader> orders = _orderRepo.GetAll(o => o.UserId == userId);
            return Json(new { data = orders });
        }
    }
}
