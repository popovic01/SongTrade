using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
                ModelState.AddModelError("Username.Name", "Username already exists");
                TempData["error"] = "Username already exists";
                return View(request);
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User();
            user.TypeOfUser = request.TypesOfUser;
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _userRepo.Add(user);
            _userRepo.Save();

            string token = CreateToken(user);
            HttpContext.Session.SetString(StaticDetails.UserToken, token);
            HttpContext.Session.SetString(StaticDetails.Role, user.TypeOfUser);

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
                if (VerifyPasswordHash(request.Password, userFromDb.PasswordHash, userFromDb.PasswordSalt))
                {
                    string token = CreateToken(userFromDb);
                    HttpContext.Session.SetString(StaticDetails.UserToken, token);
                    HttpContext.Session.SetString(StaticDetails.Role, userFromDb.TypeOfUser);

                    TempData["success"] = "You logged in successfully";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(request);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["success"] = "You logged out successfully";
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("Jwt:Token").Value)); //key from appsettings.json

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private bool ValidateCurrentToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _config.GetSection("Jwt:Token").Value)); //key from appsettings.json

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    IssuerSigningKey = key
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string GetClaim(string token, string claimType)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var ClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
            return ClaimValue;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key; 
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash); //comparing saved and computer hash
            }
        }
    }
}
