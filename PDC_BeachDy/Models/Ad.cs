using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDC_BeachDy.Models
{

    public class Ad
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public bool PriceOnCall { get; set; }
        public string Description { get; set; }
        public IEnumerable<HttpPostedFileBase> ImageFiles { get; set; }

        //public List<IFormFile> ImageFiles { get; set; }  // ✅ Change from single to multiple
        public string ImagePath { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool AgreeToTerms { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Show ad posted date
        public List<AdImage> AdImages { get; set; } = new List<AdImage>();
        public int UserId { get; set; }           // Foreign key
        public string Status { get; set; } = "Active";  // Default status

        public List<Review> Reviews { get; set; } = new List<Review>();

        public List<string> Images { get; set; }
        public int FavoritesCount { get; set; }
        public AdBiddingInfo BiddingInfo { get; set; }
    }

    public class AdImage
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public string ImagePath { get; set; }
    }


    public class AdUpdateModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public decimal? Price { get; set; }
        public bool PriceOnCall { get; set; }
        public string Description { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool AgreeToTerms { get; set; }
        public string Status { get; set; }
    }

    public class Bid
    {
        public int BidId { get; set; }
        public int AdId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; }


        public string UserName { get; set; }
    }
    public class AdBiddingInfo
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public decimal StartingAmount { get; set; }
    }


}