using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SongTrade.Authorization;
using SongTrade.DataAccess.Data;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;
using System.IdentityModel.Tokens.Jwt;

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

        [AuthRole("Role", "buyer")]
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

        public IActionResult NotAuthorized()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Song song)
        {
            song.UserId = 2;
            _songRepo.Add(song);
            _songRepo.Save();

            return RedirectToAction("Index", "Home"); //modify later
        }
    }
}
