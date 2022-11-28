using Microsoft.AspNetCore.Mvc;
using SongTrade.Authorization;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;

namespace SongTrade.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IUserRepository _userRepo;

        public SongController(ISongRepository songRepo, IUserRepository userRepo)
        {
            _songRepo = songRepo;   
            _userRepo = userRepo;
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
        public IActionResult Add(Song song)
        {
            var userId = HttpContext.Session.GetString("UserId");
            song.UserId = int.Parse(userId);
            _songRepo.Add(song);
            _songRepo.Save();

            return RedirectToAction("Index", "Home"); //modify later
        }

        [AuthRole("Role", "author")]
        public IActionResult GetByUser(string userId)
        {
            IEnumerable<Song> songs = _songRepo.GetAll(s => s.UserId == int.Parse(userId), includeProperties: "User");
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
