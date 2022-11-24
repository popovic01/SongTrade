using SongTrade.DataAccess.Data;
using SongTrade.DataAccess.Repository.IRepository;
using SongTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.DataAccess.Repository
{
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        private readonly ApplicationDbContext _db;
        public AuthorRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Author author)
        {
            _db.Authors.Update(author);
        }
    }
}
