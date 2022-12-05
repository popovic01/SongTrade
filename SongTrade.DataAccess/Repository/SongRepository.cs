using SongTrade.DataAccess.Data;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
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
    }
}
