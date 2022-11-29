using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SongTrade.Auth;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;using System.Text;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;

namespace SongTrade.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth, IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _auth = auth;
        }

        //register
        public IActionResult Index()
        {       
            return View();
        }

        //register - post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(UserDto request)
        {
            var userFromDb = _userRepo.GetFirstOrDefault(u => u.Username == request.Username);

            if (userFromDb != null)
            {
                ModelState.AddModelError("Username.Name", "Username already exists");
                return View(request);
            }

            _auth.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User();
            user.TypeOfUser = request.TypesOfUser;
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Name = request.Name;

            _userRepo.Add(user);
            _userRepo.Save();

            string token = _auth.CreateToken(user);
            HttpContext.Session.SetString(StaticDetails.UserToken, token);
            HttpContext.Session.SetString(StaticDetails.Role, user.TypeOfUser);
            HttpContext.Session.SetString(StaticDetails.UserId, user.Id.ToString());

            TempData["success"] = "You register successfully";

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(UserDto request)
        {
            var userFromDb = _userRepo.GetFirstOrDefault(u => u.Username == request.Username);

            if (userFromDb != null)
            {
                if (_auth.VerifyPasswordHash(request.Password, userFromDb.PasswordHash, userFromDb.PasswordSalt))
                {
                    string token = _auth.CreateToken(userFromDb);
                    HttpContext.Session.SetString(StaticDetails.UserToken, token);
                    HttpContext.Session.SetString(StaticDetails.Role, userFromDb.TypeOfUser);
                    HttpContext.Session.SetString(StaticDetails.UserId, userFromDb.Id.ToString());

                    TempData["success"] = "You logged in successfully";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("User.Password", "Password is not correct");
                }
            }
            else
            {
                ModelState.AddModelError("User.Username", "Username doesn't exist");
            }

            return View(request);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["success"] = "You logged out successfully";
            return RedirectToAction("Index", "Home");
        }
 
    }
}
