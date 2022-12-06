using SongTrade.DataAccess.Data;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using SongTrade.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SongTrade.DataAccess.Repository
{
    public class SongRepository : Repository<Song>, ISongRepository
    {
        private readonly ApplicationDbContext _db;

        public SongRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Song song)
        {
            _db.Songs.Update(song);
        }

        public IEnumerable<Song> GetByPage(string query, int pageNumber, int pageSize)
        {
            IEnumerable<Song> songs;
            if (query != null)
                songs = base.GetAll(s => s.Title.Contains(query) || s.User.Username.Contains(query), includeProperties: "User");
            else
                songs = base.GetAll(includeProperties: "User");
            //pagination
            songs = songs.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return songs;
        }
    }
}
