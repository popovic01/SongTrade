using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Models.ViewModel;
using SongTrade.Utility;
using Stripe.Checkout;

namespace SongTrade.Controllers
{
    public class CartController : Controller
    {
        private readonly IShoppingCartRepository _cartRepo;
        private readonly IOrderHeaderRepository _orderHeaderRepo;
        private readonly IOrderDetailRepository _orderDetailsRepo;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IShoppingCartRepository cartRepo, IOrderHeaderRepository orderHeaderRepo, IOrderDetailRepository orderDetailsRepo)
        {
            _cartRepo = cartRepo;
            _orderHeaderRepo = orderHeaderRepo;
            _orderDetailsRepo = orderDetailsRepo;
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
            var userIdString = HttpContext.Session.GetString(StaticDetails.UserId);
            var userId = int.Parse(userIdString);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCarts = _cartRepo.GetAll(c => c.UserId == userId,
                    includeProperties: "Song"),
                OrderHeader = new()
                {
                    CreatedAt = DateTime.Now,
                    UserId = userId
                }
            };

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += item.Song.Price;
            }

            _orderHeaderRepo.Add(ShoppingCartVM.OrderHeader);
            _orderHeaderRepo.Save();

            //stripe payment
            var domain = "https://localhost:44348/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"cart/index?buyerId={userIdString}"
            };

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                var sesstionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Song.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Song.Title
                        }
                    },
                    Quantity = 1
                };
                options.LineItems.Add(sesstionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;
            ShoppingCartVM.OrderHeader.SessionId = session.Id;

            //saving order to db
            _orderHeaderRepo.Update(ShoppingCartVM.OrderHeader);
            _orderHeaderRepo.Save();

            //saving order details to db
            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                OrderDetail detail = new OrderDetail()
                {
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    SongId = item.SongId,
                    Price = item.Song.Price
                };
                _orderDetailsRepo.Add(detail);
                _orderDetailsRepo.Save();
            }
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
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
            int userId = int.Parse(HttpContext.Session.GetString(StaticDetails.UserId));
            IEnumerable<ShoppingCart> carts = _cartRepo.GetAll(c => c.UserId == userId);
            _cartRepo.RemoveRange(carts);
            _cartRepo.Save();
            return View(id);
        }
    }
}
