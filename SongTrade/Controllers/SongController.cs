using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SongTrade.DataAccess.Data;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;

namespace SongTrade.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IAuthorRepository _authorRepo;

        public SongController(ISongRepository songRepo, IAuthorRepository authorRepo)
        {
            _songRepo = songRepo;   
            _authorRepo = authorRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Song> songs = _songRepo.GetAll(includeProperties: "Author");
            return View(songs);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Song song)
        {
            song.AuthorId = 2;
            _songRepo.Add(song);
            _songRepo.Save();

            return RedirectToAction("Index", "Home"); //modify later
        }
    }
}
