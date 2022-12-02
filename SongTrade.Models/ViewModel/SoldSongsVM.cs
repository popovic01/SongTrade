using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.Models.ViewModel
{
    public class SoldSongsVM
    {
        public string Title { get; set; }
        public int SoldCount { get; set; }
        public double TotalPrice { get; set; }
    }
}
