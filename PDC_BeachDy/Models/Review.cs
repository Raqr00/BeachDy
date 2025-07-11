using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDC_BeachDy.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int AdId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductDetailViewModel
    {
        public Ad Ad { get; set; }
        public List<Review> Reviews { get; set; }
    }


}