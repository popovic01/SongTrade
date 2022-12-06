﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using NuGet.Packaging.Signing;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Models.ViewModel;
using SongTrade.Utility;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace SongTrade.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IShoppingCartRepository _cartRepo;
        private readonly IOrderHeaderRepository _orderHeaderRepo;
        private readonly IOrderDetailRepository _orderDetailsRepo;
        [BindProperty]
        public SearchSongsVM SearchSongsVM { get; set; }

        public SongController(ISongRepository songRepo, IWebHostEnvironment hostEnvironment, IShoppingCartRepository cartRepo, IOrderDetailRepository orderDetailsRepo, IOrderHeaderRepository orderHeaderRepo)
        {
            _songRepo = songRepo;   
            _hostEnvironment = hostEnvironment;
            _cartRepo = cartRepo;
            _orderHeaderRepo = orderHeaderRepo;
            _orderDetailsRepo = orderDetailsRepo;
        }

        public IActionResult Index(string query, int pageNumber = 1, int pageSize = 6)
        {
            SearchSongsVM = new SearchSongsVM()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchString = query,
                Songs = _songRepo.GetByPage(query, pageNumber, pageSize)
            };
            return View(SearchSongsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SearchSongsVM vm, int pageNumber = 1, int pageSize = 6)
        {
            SearchSongsVM.Songs = _songRepo.GetByPage(vm.SearchString, pageNumber, pageSize);
            return View(SearchSongsVM);
        }

        [AuthRole("Role", "author")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Song song, IFormFile? demoUrl, IFormFile songUrl)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);
            song.UserId = int.Parse(userId);

            //demo url
            string wwRootPath = _hostEnvironment.WebRootPath;
            if (demoUrl != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwRootPath, @"files\demos");
                var extension = Path.GetExtension(demoUrl.FileName);

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    demoUrl.CopyTo(fileStreams);
                }
                song.DemoUrl = @"\files\demos\" + fileName + extension;
            }

            string songUrlName = Guid.NewGuid().ToString();
            var uploadsSong = Path.Combine(wwRootPath, @"files\songs");
            var extensionSong = Path.GetExtension(songUrl.FileName);

            using (var fileStreams = new FileStream(Path.Combine(uploadsSong, songUrlName + extensionSong), FileMode.Create))
            {
                songUrl.CopyTo(fileStreams);
            }
            song.SongUrl = @"\files\songs\" + songUrlName + extensionSong; //for db

            _songRepo.Add(song);
            _songRepo.Save();
            TempData["success"] = "Song added successfully";

            return RedirectToAction("GetByUser", "Song", new { userId }); 
        }

        [AuthRole("Role", "author")]
        public IActionResult Edit(int id)
        {
            var songFromDb = _songRepo.GetFirstOrDefault(s => s.Id == id, includeProperties: "User");

            return View(songFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Song song, IFormFile? demoUrl, IFormFile songUrl)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);
            song.UserId = int.Parse(userId);

            //demo
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (demoUrl != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"files\demos");
                var extension = Path.GetExtension(demoUrl.FileName);

                //deleting old demo url
                if (song.DemoUrl != null)
                {
                    var oldDemoPath = Path.Combine(wwwRootPath, song.DemoUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldDemoPath))
                    {
                        System.IO.File.Delete(oldDemoPath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    demoUrl.CopyTo(fileStreams);
                }
                song.DemoUrl = @"\files\demos\" + fileName + extension;
            }

            //song
            if (songUrl != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"files\songs");
                var extension = Path.GetExtension(songUrl.FileName);

                //deleting old song url
                var oldSongPath = Path.Combine(wwwRootPath, song.SongUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldSongPath))
                {
                    System.IO.File.Delete(oldSongPath);
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    songUrl.CopyTo(fileStreams);
                }
                song.DemoUrl = @"\files\songs\" + fileName + extension;
            }

            _songRepo.Update(song);
            _songRepo.Save();
            TempData["success"] = "Song updated successfully";

            return RedirectToAction("GetByUser", "Song", new { userId });
        }

        [AuthRole("Role", "author")]
        public IActionResult Delete(int? id)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);

            var songFromFb = _songRepo.GetFirstOrDefault(s => s.Id == id);
            _songRepo.Remove(songFromFb);
            _songRepo.Save();

            return RedirectToAction("GetByUser", "Song", new { userId });
        }

        [AuthRole("Role", "author")]
        public IActionResult GetByUser(string userId)
        {
            int id = int.Parse(userId);
            IEnumerable<Song> songs = _songRepo.GetAll(s => s.UserId == id, includeProperties: "User");
            return View(songs);
        }

        [AuthRole("Role", "buyer")]
        public IActionResult GetByBuyer(string buyerId)
        {
            int id = int.Parse(buyerId);
            var orders = _orderHeaderRepo.GetAll(o => o.UserId == id); //orders of buyer
            List<Song> songs = new List<Song>();
            foreach (var item in orders)
            {
                IEnumerable<OrderDetail> detailsForOrder = _orderDetailsRepo.GetAll(d => d.OrderId == item.Id, includeProperties: "Song"); //order details for every order
                foreach (var detail in detailsForOrder)
                {
                    songs.AddRange(_songRepo.GetAll(s => s.Id == detail.SongId, includeProperties: "User"));
                }
            }
            return View(songs);
        }

        public IActionResult Details(int songId)
        {
            ShoppingCart cart = new()
            {
                SongId = songId,
                Song = _songRepo.GetFirstOrDefault(s => s.Id == songId, includeProperties: "User")
            };

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart cart)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);
            cart.UserId = int.Parse(userId);
            var cartFromDb = _cartRepo.GetFirstOrDefault(c => c.UserId == int.Parse(userId) &&
                c.SongId == cart.SongId);
            var orderDetailFromDb = _orderDetailsRepo.GetFirstOrDefault(d => d.SongId == cart.SongId &&
                d.OrderHeader.UserId == int.Parse(userId));
            if (cartFromDb != null)
                TempData["success"] = "You already have this song in the shopping cart";
            else if (orderDetailFromDb != null)
                TempData["success"] = "You already bought this song";
            else
            {
                _cartRepo.Add(cart);
                _cartRepo.Save();
                TempData["success"] = "Successfully added song in the shopping cart";
            }

            return RedirectToAction("Index");
        }


        public IActionResult DetailsBoughtSong(int songId)
        {
            var songFromDb = _songRepo.GetFirstOrDefault(s => s.Id == songId, includeProperties: "User");
            return View(songFromDb);
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public IActionResult SoldSongs()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetSoldSongs()
        {
            int userId = int.Parse(HttpContext.Session.GetString(StaticDetails.UserId));
            IEnumerable<OrderDetail> details = _orderDetailsRepo.GetAll(includeProperties: "Song");
            List<SoldSongsVM> songs = new List<SoldSongsVM>();
            
            foreach (var item in details)
            {
                if (item.Song.UserId == userId)
                {
                    var count = _orderDetailsRepo.GetAll(d => d.Song == item.Song).Count();
                    var song = new SoldSongsVM
                    {
                        Title = item.Song.Title,
                        SoldCount = count,
                        TotalPrice = item.Price * count
                    };
                    if (!songs.Exists(s => s.Title == song.Title))
                        songs.Add(song);
                }
            }
            return Json(new { data = songs });
        }
    }
}
