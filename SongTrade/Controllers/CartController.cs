using Microsoft.AspNetCore.Mvc;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Models.ViewModel;
using SongTrade.Utility;

namespace SongTrade.Controllers
{
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository _cartRepo;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IShoppingCartRepository cartRepo)
        {
            _cartRepo = cartRepo;   
        }

        [AuthRole("Role", "buyer")]
        public IActionResult Index(string buyerId)
        {
            if (buyerId != null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ShoppingCarts = _cartRepo.GetAll(c => c.UserId == int.Parse(buyerId), includeProperties: "Song"),
                    OrderHeader = new()
                };
                foreach (var item in ShoppingCartVM.ShoppingCarts)
                {
                    ShoppingCartVM.OrderHeader.OrderTotal += item.Song.Price;
                }
                return View(ShoppingCartVM);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPOST()
        {
            return RedirectToAction("OrderConfirmation", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _cartRepo.GetFirstOrDefault(c => c.Id == cartId);
            _cartRepo.Remove(cartFromDb);
            _cartRepo.Save();
            var buyerId = cartFromDb.UserId.ToString();
            return RedirectToAction("Index", new { buyerId });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
