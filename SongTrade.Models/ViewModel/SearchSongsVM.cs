using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.Models.ViewModel
{
    public class SearchSongsVM
    {
        public IEnumerable<Song> Songs { get; set; }
        public string SearchString { get; set; }    
    }
}
