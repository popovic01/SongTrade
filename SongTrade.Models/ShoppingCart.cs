using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        [ForeignKey("SongId")]
        [ValidateNever]
        public Song Song { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User? User { get; set; }
    }
}
