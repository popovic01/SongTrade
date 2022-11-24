using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [ValidateNever]
        [Display(Name = "Image Url")]
        public string ImageUrl { get; set; }
        public IEnumerable<Song> Songs { get; set; }
    }
}
