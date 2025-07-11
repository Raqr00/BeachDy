using PDC_BeachDy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PDC_BeachDy.Controllers
{
    public class ValuesController : ApiController
    {
        DBAccess db = new DBAccess();
        // GET: api/Values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Values
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Values/5
        public void Delete(int id)
        {
        }

        [HttpPost]
        [Route("api/signup")]
        public IHttpActionResult SignUp([FromBody] User user)
        {
            try
            {
                // Basic password "hashing" (not secure – consider using real hash algorithms like SHA256)
                string hashedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(user.Password));

                string query = "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash)";

                db.OpenConnection();
                db.cmd = new SqlCommand(query, db.con);
                db.cmd.Parameters.AddWithValue("@Username", user.Username);
                db.cmd.Parameters.AddWithValue("@Email", user.Email);
                db.cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                db.cmd.ExecuteNonQuery();
                db.CloseConnection();

                return Ok("User inserted successfully via API.");
            }
            catch (Exception ex)
            {
                return BadRequest("API Error: " + ex.Message);
            }
        }



        [HttpPost]
        [Route("api/signin")]
        public IHttpActionResult SignIn([FromBody] User login)
        {
            try
            {
                string query = $"SELECT * FROM Users WHERE Email = '{login.Email}'";

                db.OpenConnection();
                var reader = db.getData(query);
                if (reader.HasRows)
                {
                    reader.Read();
                    string storedPasswordHash = reader["PasswordHash"].ToString();

                    // Password comparison
                    bool isPasswordCorrect = storedPasswordHash == Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(login.Password));

                    if (isPasswordCorrect)
                    {
                        var user = new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Username = reader["Username"].ToString(),
                            Email = reader["Email"].ToString()
                            // add more fields as needed
                        };

                        return Ok(user); // send back full user object


                    }
                    else
                    {
                        return BadRequest("Invalid password");
                    }
                }
                else
                {
                    return BadRequest("User not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("API Error: " + ex.Message);
            }
            finally
            {
                db.CloseConnection();
            }
        }


        //[HttpPost]
        //[Route("api/post_ad")]
        //public async Task<IHttpActionResult> Post_Ad()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        var form = httpRequest.Form;

        //        var ad = new Ad
        //        {
        //            Title = form["Title"],
        //            Category = form["Category"],
        //            Price = string.IsNullOrEmpty(form["Price"]) ? (decimal?)null : Convert.ToDecimal(form["Price"]),
        //            PriceOnCall = Convert.ToBoolean(form["PriceOnCall"]),
        //            Description = form["Description"],
        //            FirstName = form["FirstName"],
        //            LastName = form["LastName"],
        //            Phone = form["Phone"],
        //            Email = form["Email"],
        //            Address = form["Address"],
        //            Country = form["Country"],
        //            City = form["City"],
        //            AgreeToTerms = Convert.ToBoolean(form["AgreeToTerms"])
        //        };

        //        var postedFile = httpRequest.Files["ImageFile"];
        //        if (postedFile != null && postedFile.ContentLength > 0)
        //        {
        //            string fileName = Path.GetFileName(postedFile.FileName);
        //            string wwwrootPath = HttpContext.Current.Server.MapPath("~/wwwroot/Images/");
        //            if (!Directory.Exists(wwwrootPath))
        //            {
        //                Directory.CreateDirectory(wwwrootPath);
        //            }

        //            string fullPath = Path.Combine(wwwrootPath, fileName);
        //            postedFile.SaveAs(fullPath);

        //            ad.ImagePath = "Images/" + fileName;
        //        }
        //        else
        //        {
        //            ad.ImagePath = null;
        //        }

        //        // Save to database
        //        string query = @"
        //    INSERT INTO Ads 
        //    (Title, Category, Price, PriceOnCall, Description, ImagePath, 
        //     FirstName, LastName, Phone, Email, Address, Country, City, AgreeToTerms) 
        //    VALUES 
        //    (@Title, @Category, @Price, @PriceOnCall, @Description, @ImagePath,
        //     @FirstName, @LastName, @Phone, @Email, @Address, @Country, @City, @AgreeToTerms)";

        //        db.OpenConnection();
        //        db.cmd = new SqlCommand(query, db.con);
        //        db.cmd.Parameters.AddWithValue("@Title", ad.Title);
        //        db.cmd.Parameters.AddWithValue("@Category", ad.Category);
        //        db.cmd.Parameters.AddWithValue("@Price", (object)ad.Price ?? DBNull.Value);
        //        db.cmd.Parameters.AddWithValue("@PriceOnCall", ad.PriceOnCall);
        //        db.cmd.Parameters.AddWithValue("@Description", ad.Description);
        //        db.cmd.Parameters.AddWithValue("@ImagePath", (object)ad.ImagePath ?? DBNull.Value);
        //        db.cmd.Parameters.AddWithValue("@FirstName", ad.FirstName);
        //        db.cmd.Parameters.AddWithValue("@LastName", ad.LastName);
        //        db.cmd.Parameters.AddWithValue("@Phone", ad.Phone);
        //        db.cmd.Parameters.AddWithValue("@Email", ad.Email);
        //        db.cmd.Parameters.AddWithValue("@Address", ad.Address);
        //        db.cmd.Parameters.AddWithValue("@Country", ad.Country);
        //        db.cmd.Parameters.AddWithValue("@City", ad.City);
        //        db.cmd.Parameters.AddWithValue("@AgreeToTerms", ad.AgreeToTerms);
        //        db.cmd.ExecuteNonQuery();
        //        db.CloseConnection();

        //        return Ok("Ad posted successfully with image.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest("Error: " + ex.Message);
        //    }
        //}



        [HttpPost]
        [Route("api/post_ad")]
        public async Task<IHttpActionResult> Post_Ad()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var form = httpRequest.Form;

                int userId = Convert.ToInt32(form["UserId"]);
                var ad = new Ad
                {
                    Title = form["Title"],
                    Category = form["Category"],
                    Price = string.IsNullOrEmpty(form["Price"]) ? (decimal?)null : Convert.ToDecimal(form["Price"]),
                    PriceOnCall = Convert.ToBoolean(form["PriceOnCall"]),
                    Description = form["Description"],
                    FirstName = form["FirstName"],
                    LastName = form["LastName"],
                    Phone = form["Phone"],
                    Email = form["Email"],
                    Address = form["Address"],
                    Country = form["Country"],
                    City = form["City"],
                    AgreeToTerms = Convert.ToBoolean(form["AgreeToTerms"])
                };

                // Save Ad first (without image path)

                // Modified INSERT query with UserId
                string insertAdQuery = @"
INSERT INTO Ads 
(UserId, Title, Category, Price, PriceOnCall, Description, ImagePath, 
 FirstName, LastName, Phone, Email, Address, Country, City, AgreeToTerms) 
OUTPUT INSERTED.Id
VALUES 
(@UserId, @Title, @Category, @Price, @PriceOnCall, @Description, NULL,
 @FirstName, @LastName, @Phone, @Email, @Address, @Country, @City, @AgreeToTerms)";

                db.OpenConnection();
                db.cmd = new SqlCommand(insertAdQuery, db.con);
                db.cmd.Parameters.AddWithValue("@UserId", userId);
                db.cmd.Parameters.AddWithValue("@Title", ad.Title);
                db.cmd.Parameters.AddWithValue("@Category", ad.Category);
                db.cmd.Parameters.AddWithValue("@Price", (object)ad.Price ?? DBNull.Value);
                db.cmd.Parameters.AddWithValue("@PriceOnCall", ad.PriceOnCall);
                db.cmd.Parameters.AddWithValue("@Description", ad.Description);
                db.cmd.Parameters.AddWithValue("@FirstName", ad.FirstName);
                db.cmd.Parameters.AddWithValue("@LastName", ad.LastName);
                db.cmd.Parameters.AddWithValue("@Phone", ad.Phone);
                db.cmd.Parameters.AddWithValue("@Email", ad.Email);
                db.cmd.Parameters.AddWithValue("@Address", ad.Address);
                db.cmd.Parameters.AddWithValue("@Country", ad.Country);
                db.cmd.Parameters.AddWithValue("@City", ad.City);
                db.cmd.Parameters.AddWithValue("@AgreeToTerms", ad.AgreeToTerms);

                int adId = (int)db.cmd.ExecuteScalar();

                // Save uploaded images
                string wwwrootPath = HttpContext.Current.Server.MapPath("~/wwwroot/Images/");
                if (!Directory.Exists(wwwrootPath))
                    Directory.CreateDirectory(wwwrootPath);

                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    var file = httpRequest.Files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(Guid.NewGuid().ToString() + "_" + file.FileName);
                        string fullPath = Path.Combine(wwwrootPath, fileName);
                        file.SaveAs(fullPath);

                        // First image becomes main ImagePath (optional)
                        if (i == 0)
                        {
                            string updateImagePathQuery = "UPDATE Ads SET ImagePath = @ImagePath WHERE Id = @AdId";
                            using (var updateCmd = new SqlCommand(updateImagePathQuery, db.con))
                            {
                                updateCmd.Parameters.AddWithValue("@ImagePath", "Images/" + fileName);
                                updateCmd.Parameters.AddWithValue("@AdId", adId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }

                        // Insert each image path into AdImages table
                        string insertImageQuery = "INSERT INTO AdImages (AdId, ImagePath) VALUES (@AdId, @ImagePath)";
                        using (var imageCmd = new SqlCommand(insertImageQuery, db.con))
                        {
                            imageCmd.Parameters.AddWithValue("@AdId", adId);
                            imageCmd.Parameters.AddWithValue("@ImagePath", "Images/" + fileName);
                            imageCmd.ExecuteNonQuery();
                        }
                    }
                }

                db.CloseConnection();
                return Ok("Ad posted successfully with multiple images.");
            }
            catch (Exception ex)
            {
                db.CloseConnection();
                return BadRequest("Error: " + ex.Message);
            }
        }






        [HttpGet]
        [Route("api/GetAllAds")]
        public IHttpActionResult GetAllAds()
        {
            try
            {
                var ads = db.GetAllAds(); // same SQL code as before
                return Ok(ads);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        [HttpGet]
        [Route("api/GetAdById/{id}")]
        public IHttpActionResult GetAdById(int id)
        {
            Ad ad = db.GetAdById(id);

            if (ad == null)
                return NotFound();

            return Ok(ad);
        }




        [HttpGet]
        [Route("api/GetAdsByUserId/{userId}")]
        public IHttpActionResult GetAdsByUserId(int userId)
        {
            var ads = db.GetAdsByUserId(userId);
            if (ads == null || !ads.Any())
                return NotFound();

            return Ok(ads);
        }



        [HttpPost]
        [Route("api/AddToFavorites")]
        public IHttpActionResult AddToFavorites([FromBody] Favorites request)
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.AdId <= 0)
                    return BadRequest("Invalid request data");

                db.AddToFavorites(request.UserId, request.AdId);
                return Ok("Added to favorites");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpGet]
        [Route("api/GetFavoriteAds/{userId}")]
        public IHttpActionResult GetFavoriteAds(int userId)
        {
            try
            {
                var favoriteAds = db.GetFavoriteAdsByUser(userId);
                if (favoriteAds == null || !favoriteAds.Any())
                    return Ok(new List<Ad>());

                return Ok(favoriteAds);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        [HttpGet]
        [Route("api/GetAdByIdForEdit/{id}")]
        public IHttpActionResult GetAdByIdForEdit(int id)
        {
            try
            {
                var ad = db.GetAdById(id);
                if (ad == null)
                    return NotFound();

                return Ok(ad);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }




        [HttpPut]
        [Route("api/UpdateAd")]
        public IHttpActionResult UpdateAd([FromBody] Ad ad)
        {
            if (ad == null)
                return BadRequest("Invalid ad data");

            try
            {
                db.UpdateAd(ad);
                return Ok("Ad updated successfully");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }





        [HttpGet]
        [Route("api/GetAdByIdForDelete/{id}")]
        public IHttpActionResult GetAdByIdForDelete(int id)
        {
            var ad = db.GetAdById(id);
            if (ad == null)
                return NotFound();

            return Ok(ad);
        }




        [HttpGet]
        [Route("api/GetReviewsByAdId/{adId}")]
        public IHttpActionResult GetReviewsByAdId(int adId)
        {
            List<Review> reviews = new List<Review>();

            db.OpenConnection();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Reviews WHERE AdId = @AdId", db.con);
            cmd.Parameters.AddWithValue("@AdId", adId);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reviews.Add(new Review
                {
                    ReviewId = Convert.ToInt32(reader["ReviewId"]),
                    AdId = Convert.ToInt32(reader["AdId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Rating = Convert.ToInt32(reader["Rating"]),
                    Name = reader["Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    ReviewText = reader["ReviewText"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }
            db.CloseConnection();

            return Ok(reviews);
        }



        [HttpGet]
        [Route("api/GetBiddingInfo/{adId}")]
        public IHttpActionResult GetBiddingInfo(int adId)
        {
            var bidding = db.GetAdBiddingInfo(adId); // This must return StartingAmount too
            if (bidding == null)
                return NotFound();

            return Ok(bidding);
        }

        [HttpGet]
        [Route("api/GetBiddingWinner/{adId}")]
        public IHttpActionResult GetBiddingWinner(int adId)
        {
            var winner = db.GetBiddingWinner(adId);
            if (winner == null) return NotFound();
            return Ok(winner);
        }




    }
}
