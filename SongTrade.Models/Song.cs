using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SongTrade.Models
{
    public class Song
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int Price { get; set; }
        public string Lyrics { get; set; }
        public DateTime Published { get; set; }
        [Required]
        [Display(Name = "Author")]
        public int UserId { get; set; }
        [ValidateNever]
        public User User { get; set; }
        [Display(Name = "Demo")]
        [ValidateNever]
        public string DemoUrl { get; set; }
    }
}
