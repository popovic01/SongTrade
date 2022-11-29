using Microsoft.AspNetCore.Mvc;
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

        public SongController(ISongRepository songRepo, IWebHostEnvironment hostEnvironment)
        {
            _songRepo = songRepo;   
            _hostEnvironment = hostEnvironment;
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
            TempData["success"] = "Song deleted successfully";

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
            var songFromDb = _songRepo.GetFirstOrDefault(s => s.Id == songId, includeProperties: "User");
            return View(songFromDb);
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }
    }
}
