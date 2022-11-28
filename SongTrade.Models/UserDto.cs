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
        [MinLength(8, ErrorMessage = "Password must be at least 8 charasters long")]
        public string Password { get; set; }
        [Display(Name = "Type of User")]
        public string TypesOfUser { get; set; }
        [MinLength(8, ErrorMessage = "Password must be at least 8 charasters long")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
    }
}
