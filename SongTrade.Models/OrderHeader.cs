using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongTrade.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
        public double OrderTotal { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
    }
}
