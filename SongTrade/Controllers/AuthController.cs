using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web.Mvc;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;

namespace SongTrade.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config, IUserRepository userRepo)
        {
            _config = config;
            _userRepo = userRepo;
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
                throw new Exception();
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User();
            user.TypeOfUser = request.TypesOfUser;
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _userRepo.Add(user);
            _userRepo.Save();

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
                if (VerifyPasswordHash(request.Password, userFromDb.PasswordHash, userFromDb.PasswordSalt))
                {
                    string token = CreateToken(userFromDb);
                    //adding token to session
                    HttpContext.Session.SetString(StaticDetails.UserToken, token);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private string CreateToken(User user)
        {
            //properties for token
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Role", user.TypeOfUser)
            }; 

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetSection("Jwt:Token").Value)); //key from appsettings.json

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; 
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash); //comparing saved and computer hash
            }
        }
    }
}
