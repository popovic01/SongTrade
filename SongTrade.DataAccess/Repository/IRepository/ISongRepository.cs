using SongTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.DataAccess.Repository.IRepository
{
    public interface ISongRepository : IRepository<Song>
    {
        void Update(Song song);
        IEnumerable<Song> GetByPage(string query, int pageNumber, int pageSize, string sort);
    }
}
