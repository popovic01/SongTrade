using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SongTrade.Models
{
    //external use
    public class UserDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Display(Name = "Type of User")]
        public string TypesOfUser { get; set; }
    }
}
