using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDC_BeachDy.Models
{
    public class Favorites
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AdId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}