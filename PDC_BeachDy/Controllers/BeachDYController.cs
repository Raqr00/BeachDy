using Newtonsoft.Json;
using PDC_BeachDy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PDC_BeachDy.Controllers
{
    public class BeachDYController : Controller
    {

        DBAccess db = new DBAccess();
        // GET: BeachDY
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Home()
        {
            ViewBag.PageTitle = "Home";
            List<Ad> ads = db.GetTop8Ads(); // Your method to fetch ads from DB
            return View(ads);
        }

        public ActionResult Post_Ad()
        {
            ViewBag.PageTitle = "Post Ad";
            return View();
        }

        //[HttpPost]
        //public async Task<ActionResult> Post_Ad(Ad model, HttpPostedFileBase ImageFile)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        using (var form = new MultipartFormDataContent())
        //        {
        //            // Add form fields
        //            form.Add(new StringContent(model.Title ?? ""), "Title");
        //            form.Add(new StringContent(model.Category ?? ""), "Category");
        //            form.Add(new StringContent(model.Price?.ToString() ?? ""), "Price");
        //            form.Add(new StringContent(model.PriceOnCall.ToString()), "PriceOnCall");
        //            form.Add(new StringContent(model.Description ?? ""), "Description");
        //            form.Add(new StringContent(model.FirstName ?? ""), "FirstName");
        //            form.Add(new StringContent(model.LastName ?? ""), "LastName");
        //            form.Add(new StringContent(model.Phone ?? ""), "Phone");
        //            form.Add(new StringContent(model.Email ?? ""), "Email");
        //            form.Add(new StringContent(model.Address ?? ""), "Address");
        //            form.Add(new StringContent(model.Country ?? ""), "Country");
        //            form.Add(new StringContent(model.City ?? ""), "City");
        //            form.Add(new StringContent(model.AgreeToTerms.ToString()), "AgreeToTerms");

        //            // Add image file
        //            if (ImageFile != null && ImageFile.ContentLength > 0)
        //            {
        //                byte[] fileBytes;
        //                using (var stream = new MemoryStream())
        //                {
        //                    ImageFile.InputStream.CopyTo(stream);
        //                    fileBytes = stream.ToArray();
        //                }

        //                var fileContent = new ByteArrayContent(fileBytes);
        //                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
        //                {
        //                    Name = "ImageFile",
        //                    FileName = ImageFile.FileName
        //                };
        //                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ImageFile.ContentType);
        //                form.Add(fileContent);
        //            }

        //            // Send request
        //            client.BaseAddress = new Uri("https://localhost:44319/"); // <-- replace with your API base URL
        //            var response = await client.PostAsync("api/post_ad", form);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                TempData["Message"] = "Ad posted successfully!";
        //                return RedirectToAction("Post_Ad"); // Or wherever you want to go after posting
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "Failed to post ad.");
        //                return View(model);
        //            }
        //        }
        //    }
        //}


        [HttpPost]
        public async Task<ActionResult> Post_Ad(Ad model, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            ViewBag.PageTitle = "Post Ad";
            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            {
                if (Session["UserId"] != null)
                {
                    form.Add(new StringContent(Session["UserId"].ToString()), "UserId");
                }
                else
                {
                    ModelState.AddModelError("", "You must be logged in to post an ad.");
                    return View(model);
                }

                // Add form fields
                form.Add(new StringContent(model.Title ?? ""), "Title");
                form.Add(new StringContent(model.Category ?? ""), "Category");
                form.Add(new StringContent(model.Price?.ToString() ?? ""), "Price");
                form.Add(new StringContent(model.PriceOnCall.ToString()), "PriceOnCall");
                form.Add(new StringContent(model.Description ?? ""), "Description");
                form.Add(new StringContent(model.FirstName ?? ""), "FirstName");
                form.Add(new StringContent(model.LastName ?? ""), "LastName");
                form.Add(new StringContent(model.Phone ?? ""), "Phone");
                form.Add(new StringContent(model.Email ?? ""), "Email");
                form.Add(new StringContent(model.Address ?? ""), "Address");
                form.Add(new StringContent(model.Country ?? ""), "Country");
                form.Add(new StringContent(model.City ?? ""), "City");
                form.Add(new StringContent(model.AgreeToTerms.ToString()), "AgreeToTerms");

                // Add multiple image files
                if (ImageFiles != null)
                {
                    foreach (var image in ImageFiles)
                    {
                        if (image != null && image.ContentLength > 0)
                        {
                            byte[] fileBytes;
                            using (var stream = new MemoryStream())
                            {
                                image.InputStream.CopyTo(stream);
                                fileBytes = stream.ToArray();
                            }

                            var fileContent = new ByteArrayContent(fileBytes);
                            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "ImageFiles",
                                FileName = image.FileName
                            };
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                            form.Add(fileContent);
                        }
                    }
                }

                // Post to API
                client.BaseAddress = new Uri("https://localhost:44319/");
                var response = await client.PostAsync("api/post_ad", form);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "Ad posted successfully!";
                    return RedirectToAction("Post_Ad");
                }

                ModelState.AddModelError("", "Failed to post ad.");
                return View(model);
            }
        }


        public ActionResult SignUp()
        {
            ViewBag.PageTitle = "Sign Up";
            return View();
        }


        [HttpPost]
        public ActionResult SignUp(User model)
        {
            ViewBag.PageTitle = "Sign Up";
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44319/"); 
                    var response = client.PostAsJsonAsync("api/signup", model).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "User registered!";
                        return RedirectToAction("SignIn"); // or wherever you want
                    }
                    else
                    {
                        ModelState.AddModelError("", "API Error: " + response.Content.ReadAsStringAsync().Result);
                    }
                }
            }

            return View(model);
        }


        public ActionResult SignIn()
        {
            ViewBag.PageTitle = "Sign In";
            return View();
        }


        [HttpPost]
        public ActionResult SignIn(string username, string password)
        {
            ViewBag.PageTitle = "Sign In";
            // Validate User credentials using API or DB (Database query or API call)
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                var loginDetails = new { Email = username, Password = password };
                var postTask = client.PostAsJsonAsync("api/signin", loginDetails);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<User>();
                    readTask.Wait();
                    var user = readTask.Result;

                    Session["UserId"] = user.UserId;
                    Session["UserName"] = user.Username;


                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    TempData["Success"] = "Login Successful!";
                    return RedirectToAction("Home"); // Or wherever you want to redirect the user after successful login
                }
                else
                {
                    string error = result.Content.ReadAsStringAsync().Result;
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return View(); // Return to the SignIn page if login fails
        }


        public ActionResult CategoryProducts(string category)
        {

            ViewBag.PageTitle = "CategoryProducts";

            var ads = db.GetAdsByCategory(category);

            ViewBag.CategoryName = category;

            if (ads.Count == 0)
            {
                ViewBag.Message = "No products found for this category.";
            }

            return View("CategoryProducts", ads);
        }




        public ActionResult Categories()
        {
            ViewBag.PageTitle = "Categories";
            return View();
        }

        //public ActionResult Product_Listing()
        //{
        //    List<Ad> ads = db.GetAllAds(); 
        //    return View(ads);
        //}

        [HttpGet]
        public async Task<ActionResult> Product_Listing()
        {
            ViewBag.PageTitle = "Product Listing";
            List<Ad> ads = new List<Ad>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/GetAllAds");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    ads = JsonConvert.DeserializeObject<List<Ad>>(json);
                }
                else
                {
                    TempData["Error"] = "API call failed. Status code: " + response.StatusCode;
                }
            }

            return View(ads);
        }


        //public ActionResult Product_Details(int id)
        //{
        //    Ad ad = db.GetAdById(id);

        //    if (ad == null)
        //        return HttpNotFound();

        //    return View(ad);
        //}


        [HttpGet]
        public async Task<ActionResult> Product_Details(int id)
        {
            ViewBag.PageTitle = "Product Detail";
            Ad ad = null;
            AdBiddingInfo biddingInfo = null;
            (string UserName, decimal Amount)? highestBid = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");

                // 1. Get Ad
                HttpResponseMessage adResponse = await client.GetAsync($"api/GetAdById/{id}");
                if (adResponse.IsSuccessStatusCode)
                {
                    ad = await adResponse.Content.ReadAsAsync<Ad>();
                }

                if (ad == null)
                    return HttpNotFound();

                // 2. Get Reviews
                HttpResponseMessage reviewResponse = await client.GetAsync($"api/GetReviewsByAdId/{id}");
                if (reviewResponse.IsSuccessStatusCode)
                {
                    var reviews = await reviewResponse.Content.ReadAsAsync<List<Review>>();
                    ad.Reviews = reviews;
                }
                else
                {
                    ad.Reviews = new List<Review>();
                }

                // 3. Get Bidding Info
                HttpResponseMessage bidInfoResponse = await client.GetAsync($"api/GetBiddingInfo/{id}");
                if (bidInfoResponse.IsSuccessStatusCode)
                {
                    biddingInfo = await bidInfoResponse.Content.ReadAsAsync<AdBiddingInfo>();
                }

                


                // 4. Get Highest Bid
                HttpResponseMessage highestBidResponse = await client.GetAsync($"api/GetBiddingWinner/{id}");
                if (highestBidResponse.IsSuccessStatusCode)
                {
                    highestBid = await highestBidResponse.Content.ReadAsAsync<(string, decimal)>();
                }
            }

            // After getting bidding info
            var recentBids = db.GetRecentBids(id);
            ViewBag.RecentBids = recentBids;

            ViewBag.BiddingInfo = biddingInfo;
            ViewBag.HighestBid = highestBid;

            return View(ad);
        }







        //public ActionResult My_Ads()
        //{
        //    if (Session["UserId"] == null)
        //    {
        //        return RedirectToAction("Login", "Auth"); // or your login page
        //    }

        //    int userId = Convert.ToInt32(Session["UserId"]);
        //    var myAds = db.GetAdsByUserId(userId);

        //    return View(myAds);
        //}



        public async Task<ActionResult> My_Ads()
        {
            ViewBag.PageTitle = "My Ads";
            if (Session["UserId"] == null)
            {
                return RedirectToAction("SignIn", "BeachDY");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            List<Ad> myAds = new List<Ad>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                HttpResponseMessage response = await client.GetAsync("api/GetAdsByUserId/" + userId);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsAsync<List<Ad>>();
                    myAds = data;
                }
            }

            return View(myAds);
        }




        public ActionResult Error_404()
        {
            ViewBag.PageTitle = "Error 404";
            return View();
        }
         public ActionResult Sign_out()
        {
            FormsAuthentication.SignOut();
            Session.Clear();    
            Session.Abandon();  

            return RedirectToAction("SignIn", "BeachDY"); 
        }


        //public ActionResult AddToFavorites(int adId)
        //{
        //    if (Session["UserId"] == null)
        //    {
        //        return RedirectToAction("Signin", "BeachDY"); // or show error
        //    }

        //    int userId = Convert.ToInt32(Session["UserId"]);

        //    try
        //    {
        //        db.AddToFavorites(userId, adId);
        //        return RedirectToAction("My_Favourite");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Log the exception or show it
        //        ViewBag.Error = ex.Message;
        //        return View("Error_404");
        //    }
        //}


        public async Task<ActionResult> AddToFavorites(int adId)
        {
            ViewBag.PageTitle = "Add To Favorites";
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Signin", "BeachDY");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                var favoriteData = new
                {
                    UserId = userId,
                    AdId = adId
                };

                HttpResponseMessage response = await client.PostAsJsonAsync("api/AddToFavorites", favoriteData);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("My_Favourite");
                }
                else
                {
                    ViewBag.Error = "Could not add to favorites.";
                    return View("Error_404");
                }
            }
        }



        //public ActionResult My_Favourite()
        //{
        //    int userId = Convert.ToInt32(Session["UserId"]);
        //    List<Ad> favoriteAds = db.GetFavoriteAdsByUser(userId) ?? new List<Ad>();

        //    return View(favoriteAds);
        //}


        public async Task<ActionResult> My_Favourite()
        {
            ViewBag.PageTitle = "My Favorite";
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Signin", "BeachDY");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            List<Ad> favoriteAds = new List<Ad>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");

                HttpResponseMessage response = await client.GetAsync("api/GetFavoriteAds/" + userId);
                if (response.IsSuccessStatusCode)
                {
                    favoriteAds = await response.Content.ReadAsAsync<List<Ad>>();
                }
            }

            return View(favoriteAds);
        }





        //public ActionResult Edit(int id)
        //{
        //    Ad ad = db.GetAdById(id); 
        //    if (ad == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ad);
        //}




        public async Task<ActionResult> Edit(int id)
        {
            ViewBag.PageTitle = "Edit Ad";
            Ad ad = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");

                HttpResponseMessage response = await client.GetAsync($"api/GetAdByIdForEdit/{id}");
                if (response.IsSuccessStatusCode)
                {
                    ad = await response.Content.ReadAsAsync<Ad>();
                }
            }

            if (ad == null)
            {
                return HttpNotFound();
            }

            return View(ad);
        }





        //[HttpPost]
        //public ActionResult Edit(Ad ad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.UpdateAd(ad); 
        //        return RedirectToAction("My_Ads");
        //    }
        //    return View(ad);
        //}


        [HttpPost]
        public async Task<ActionResult> Edit(Ad ad)
        {
            ViewBag.PageTitle = "Edit Ad";

            if (!ModelState.IsValid)
                return View(ad);

            // ✅ Create AdUpdateModel from Ad object (only simple properties)
            var updateModel = new AdUpdateModel
            {
                Id = ad.Id,
                Title = ad.Title,
                Category = ad.Category,
                Price = ad.Price,
                PriceOnCall = ad.PriceOnCall,
                Description = ad.Description,
                FirstName = ad.FirstName,
                LastName = ad.LastName,
                Phone = ad.Phone,
                Email = ad.Email,
                Address = ad.Address,
                Country = ad.Country,
                City = ad.City,
                AgreeToTerms = ad.AgreeToTerms,
                Status = ad.Status
            };

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                HttpResponseMessage response = await client.PutAsJsonAsync("api/UpdateAd", updateModel);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Something went wrong while updating the ad.");
                    return View(ad);
                }
            }

            // ✅ File Upload after successful update
            if (ad.ImageFiles != null)
            {
                foreach (var file in ad.ImageFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/wwwroot/Images/"), fileName);
                        file.SaveAs(path);

                        db.AddAdImage(ad.Id, "/Images/" + fileName);
                    }
                }
            }

            return RedirectToAction("My_Ads");
        }



        //public ActionResult Delete(int id)
        //{
        //    Ad ad = db.GetAdById(id); 
        //    if (ad == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ad);
        //}



        public async Task<ActionResult> Delete(int id)
        {
            Ad ad = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44319/");
                HttpResponseMessage response = await client.GetAsync($"api/GetAdByIdForDelete/{id}");

                if (response.IsSuccessStatusCode)
                {
                    ad = await response.Content.ReadAsAsync<Ad>();
                }
            }

            if (ad == null)
            {
                return HttpNotFound();
            }

            return View(ad);
        }


        public ActionResult DeleteConfirmed(int id)
        {

            var imagePaths = db.GetAdImagePaths(id);

            foreach (var relativePath in imagePaths)
            {
                string physicalPath = Server.MapPath("~/wwwroot" + relativePath);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            db.DeleteAd(id);

            return RedirectToAction("My_Ads");
        }


        [HttpGet]
        public ActionResult DeleteFavorite(int adId)
        {
            // Yahan se logged-in user ka ID lo
            int userId = Convert.ToInt32(Session["UserId"]);

            db.DeleteFavorite(userId, adId);

            return RedirectToAction("My_Favourite"); // apni favorites list wali screen ka naam likhna
        }





        public ActionResult DeleteImage(int adId, string imagePath)
        {
            db.DeleteAdImage(adId, imagePath);
            return RedirectToAction("Edit", new { id = adId });
        }




        [HttpPost]
        public ActionResult SubmitReview(int AdId, int rating, string name, string email, string review)
        {
            if (Session["UserId"] == null)
            {
                TempData["Error"] = "Please login to submit a review.";
                return RedirectToAction("Signin");
            }

            int UserId = (int)Session["UserId"];

            db.OpenConnection();
            SqlCommand cmd = new SqlCommand("INSERT INTO Reviews (AdId, UserId, Rating, Name, Email, ReviewText) VALUES (@AdId, @UserId, @Rating, @Name, @Email, @ReviewText)", db.con);
            cmd.Parameters.AddWithValue("@AdId", AdId);
            cmd.Parameters.AddWithValue("@UserId", UserId);
            cmd.Parameters.AddWithValue("@Rating", rating);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@ReviewText", review);
            cmd.ExecuteNonQuery();
            db.CloseConnection();

            TempData["Success"] = "Review submitted successfully!";
            return RedirectToAction("Product_Details", new { id = AdId });
        }




        [HttpGet]
        public ActionResult Search(string query)
        {
            ViewBag.PageTitle = " Search ";
            List<Ad> ads = db.SearchAds(query);  

            return View(ads); 
        }






        // Step 3: Enable Bidding Button Action for Seller
        [HttpGet]
        public ActionResult EnableBidding(int adId)
        {
            ViewBag.AdId = adId;
            return View(); // View with StartTime & EndTime input
        }

        [HttpPost]
        public ActionResult EnableBidding(int adId, DateTime startTime, DateTime endTime, decimal startingAmount)
        {
            if (startTime >= endTime)
            {
                ViewBag.Error = "End time must be greater than start time.";
                ViewBag.AdId = adId;
                return View();
            }

            db.EnableBidding(adId, startTime, endTime, startingAmount);

            return RedirectToAction("My_Ads");
        }


        [HttpPost]
        public ActionResult PlaceBid(int adId, decimal amount)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Signin", "BeachDY");

            int userId = Convert.ToInt32(Session["UserId"]);

            var result = db.AddOrUpdateBid(adId, userId, amount);

            if (result != "✅ Bid placed successfully.")
            {
                TempData["Error"] = result;
            }
            else
            {
                TempData["Success"] = result;
            }

            return RedirectToAction("Product_Details", new { id = adId });
        }





        // Step 5: Show Winner Logic
        public ActionResult BiddingWinner(int adId)
        {
            var winner = db.GetBiddingWinner(adId);
            ViewBag.AdId = adId;

            if (winner == null)
            {
                ViewBag.Message = "No bids were placed or bidding not ended yet.";
                return View();
            }

            ViewBag.WinnerName = winner?.UserName;
            ViewBag.WinningAmount = winner?.Amount;
            return View();
        }
















        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}