using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult Index()
        {
            IEnumerable<Song> songs = _songRepo.GetAll(includeProperties: "User");
            return View(songs);
        }

        //[Authorize]
        public IActionResult Add()
        {
            string token = HttpContext.Session.GetString(StaticDetails.UserToken); //get token from session
            var decodedToken = new JwtSecurityToken(jwtEncodedString: token); //decoding token
            string role = decodedToken.Claims.First(c => c.Type == "Role").Value; //get role of logged user
            HttpContext.Session.SetString("Role", role);
            if (role == "author")
                return View();
            return RedirectToAction("NotAuthorized");
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
