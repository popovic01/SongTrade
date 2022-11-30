using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;
using System.Drawing;

namespace SongTrade.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IShoppingCartRepository _cartRepo;

        public SongController(ISongRepository songRepo, IWebHostEnvironment hostEnvironment, IShoppingCartRepository cartRepo)
        {
            _songRepo = songRepo;   
            _hostEnvironment = hostEnvironment;
            _cartRepo = cartRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Song> songs = _songRepo.GetAll(includeProperties: "User");
            return View(songs);
        }

        [AuthRole("Role", "author")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Song song, IFormFile? file)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);
            song.UserId = int.Parse(userId);

            //file
            string wwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwRootPath, @"files\demos");
                var extension = Path.GetExtension(file.FileName);

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                song.DemoUrl = @"\files\demos\" + fileName + extension;
            }

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
        public IActionResult Edit(Song song, IFormFile? file)
        {
            var userId = HttpContext.Session.GetString(StaticDetails.UserId);
            song.UserId = int.Parse(userId);

            //file
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"files\demos");
                var extension = Path.GetExtension(file.FileName);

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
                    file.CopyTo(fileStreams);
                }
                song.DemoUrl = @"\files\demos\" + fileName + extension;
            }

            _songRepo.Update(song);
            _songRepo.Save();
            TempData["success"] = "Song updated successfully";

            return RedirectToAction("GetByUser", "Song", new { userId });
        }

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
            if (cartFromDb != null)
                TempData["success"] = "You already have this song in the shopping cart";
            else
            {
                _cartRepo.Add(cart);
                _cartRepo.Save();
                TempData["success"] = "You successfully added this song in the shopping cart";
            }

            return RedirectToAction("Index");
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
