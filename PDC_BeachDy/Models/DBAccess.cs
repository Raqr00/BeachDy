using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace PDC_BeachDy.Models
{
    public class DBAccess
    {
        //static shared memory 
        static string constr = "Data Source=DESKTOP-NQS5QPM\\SQLEXPRESS;Initial Catalog=BeachDy;Integrated Security=True;";
        public SqlConnection con = new SqlConnection(constr);
        public SqlCommand cmd = null;
        public SqlDataReader sdr = null;

        public void OpenConnection()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();

            }
        }
        public void CloseConnection()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Close();

            }
        }

        public List<Ad> GetTop8Ads()
        {
            List<Ad> ads = new List<Ad>();
            //string query = "SELECT TOP 8 * FROM Ads ORDER BY Id DESC"; // Limit to top 8 latest ads
            string query = "SELECT TOP 8 * FROM Ads WHERE UPPER(ISNULL(Status, '')) <> 'SOLD' ORDER BY Id DESC";

            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Ad ad = new Ad
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    Title = dr["Title"]?.ToString(),
                    Category = dr["Category"]?.ToString(),
                    Address = dr["Address"]?.ToString(),
                    Price = dr["Price"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(dr["Price"]),
                    PriceOnCall = dr["PriceOnCall"] != DBNull.Value && Convert.ToBoolean(dr["PriceOnCall"]),
                    ImagePath = dr["ImagePath"] == DBNull.Value ? null : dr["ImagePath"].ToString()
                };
                ads.Add(ad);
            }

            con.Close();
            return ads;
        }
        public List<Ad> GetAllAds()
        {
            List<Ad> ads = new List<Ad>();
            string query = "SELECT * FROM Ads";
            //string query = "SELECT * FROM Ads WHERE UPPER(TRIM(ISNULL(Status, ''))) <> 'SOLD'";

            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                Ad ad = new Ad
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    Title = dr["Title"]?.ToString(),
                    Category = dr["Category"]?.ToString(),
                    Address = dr["Address"]?.ToString(),
                    Price = dr["Price"] == DBNull.Value ? null : (decimal?)Convert.ToDecimal(dr["Price"]),
                    PriceOnCall = dr["PriceOnCall"] != DBNull.Value && Convert.ToBoolean(dr["PriceOnCall"]),
                    ImagePath = dr["ImagePath"] == DBNull.Value ? null : dr["ImagePath"].ToString()
                };
                ads.Add(ad);
            }

            con.Close();
            return ads;
        }

        public Ad GetAdById(int id)
        {
            Ad ad = null;

            using (SqlConnection conn = new SqlConnection(constr))
            {
                conn.Open();

                // 1. Fetch main ad
                SqlCommand cmd = new SqlCommand("SELECT * FROM Ads WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ad = new Ad
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString(),
                        Category = reader["Category"].ToString(),
                        Price = reader["Price"] != DBNull.Value ? (decimal?)reader["Price"] : null,
                        PriceOnCall = (bool)reader["PriceOnCall"],
                        Description = reader["Description"].ToString(),
                        ImagePath = reader["ImagePath"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Email = reader["Email"].ToString(),
                        Address = reader["Address"].ToString(),
                        Country = reader["Country"].ToString(),
                        City = reader["City"].ToString(),
                        AgreeToTerms = (bool)reader["AgreeToTerms"],
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        AdImages = new List<AdImage>(), // initialize list
                        UserId = Convert.ToInt32(reader["UserId"])

                    };
                }
                reader.Close();

                // 2. Fetch related images
                if (ad != null)
                {
                    SqlCommand imgCmd = new SqlCommand("SELECT * FROM AdImages WHERE AdId = @AdId", conn);
                    imgCmd.Parameters.AddWithValue("@AdId", ad.Id);
                    SqlDataReader imgReader = imgCmd.ExecuteReader();

                    while (imgReader.Read())
                    {
                        ad.AdImages.Add(new AdImage
                        {
                            Id = (int)imgReader["ImageId"],
                            AdId = (int)imgReader["AdId"],
                            ImagePath = imgReader["ImagePath"].ToString()
                        });
                    }
                    imgReader.Close();
                }

                conn.Close();
            }

            return ad;
        }

        public void EnableBidding(int adId, DateTime startTime, DateTime endTime, decimal startingAmount)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            INSERT INTO AdBiddingInfo (AdId, StartTime, EndTime, IsActive, StartingAmount)
            VALUES (@AdId, @StartTime, @EndTime, 1, @StartingAmount)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                cmd.Parameters.AddWithValue("@StartingAmount", startingAmount);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public void PlaceBid(int adId, int userId, decimal amount)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            IF EXISTS (SELECT 1 FROM Bids WHERE AdId = @AdId AND UserId = @UserId)
            BEGIN
                UPDATE Bids 
                SET Amount = Amount + @Amount, BidTime = GETDATE()
                WHERE AdId = @AdId AND UserId = @UserId
            END
            ELSE
            BEGIN
                INSERT INTO Bids (AdId, UserId, Amount, BidTime) 
                VALUES (@AdId, @UserId, @Amount, GETDATE())
            END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Amount", amount);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public string AddOrUpdateBid(int adId, int userId, decimal newAmount)
        {
            var ad = GetAdById(adId);
            if (ad == null)
                return "❌ Ad not found.";

            if (ad.UserId == userId)
                return "⚠️ You cannot bid on your own ad.";

            var biddingInfo = GetAdBiddingInfo(adId);
            if (biddingInfo == null || !biddingInfo.IsActive)
                return "❌ Bidding not enabled.";

            if (DateTime.Now < biddingInfo.StartTime || DateTime.Now > biddingInfo.EndTime)
                return "⏳ Bidding is not active right now.";

            // ✅ Correct call to UserHasBid
            bool userHasBid = UserHasBid(adId, userId);

            // ✅ Apply starting amount rule ONLY for first-time bidder
            if (!userHasBid && newAmount < biddingInfo.StartingAmount)
                return $"⚠️ First bid must be at least Rs. {biddingInfo.StartingAmount}.";

            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
        IF EXISTS (SELECT 1 FROM Bids WHERE AdId = @AdId AND UserId = @UserId)
        BEGIN
            UPDATE Bids 
            SET Amount = Amount + @Amount, BidTime = GETDATE()
            WHERE AdId = @AdId AND UserId = @UserId
        END
        ELSE
        BEGIN
            INSERT INTO Bids (AdId, UserId, Amount, BidTime) 
            VALUES (@AdId, @UserId, @Amount, GETDATE())
        END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Amount", newAmount);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return "✅ Bid placed successfully.";
        }



        public bool UserHasBid(int adId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = "SELECT COUNT(*) FROM Bids WHERE AdId = @AdId AND UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }




        public (string UserName, decimal Amount)? GetBiddingWinner(int adId)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            SELECT TOP 1 u.Username, b.Amount
            FROM Bids b
            INNER JOIN Users u ON b.UserId = u.UserId
            INNER JOIN AdBiddingInfo ab ON b.AdId = ab.AdId
            WHERE b.AdId = @AdId AND b.BidTime <= ab.EndTime
            ORDER BY b.Amount DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader.GetString(0), reader.GetDecimal(1));
                    }
                }
            }
            return null;
        }


        public AdBiddingInfo GetAdBiddingInfo(int adId)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = "SELECT Id, AdId, StartTime, EndTime, IsActive, StartingAmount FROM AdBiddingInfo WHERE AdId = @AdId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdBiddingInfo
                        {
                            Id = reader.GetInt32(0),
                            AdId = reader.GetInt32(1),
                            StartTime = reader.GetDateTime(2),
                            EndTime = reader.GetDateTime(3),
                            IsActive = reader.GetBoolean(4),
                            StartingAmount = reader.GetDecimal(5)  // 👈 Important
                        };
                    }
                }
            }

            return null;
        }




        public List<Bid> GetRecentBids(int adId)
        {
            List<Bid> bids = new List<Bid>();

            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            SELECT TOP 5 b.Amount, b.BidTime, u.Username
            FROM Bids b
            INNER JOIN Users u ON b.UserId = u.UserId
            WHERE b.AdId = @AdId
            ORDER BY b.BidTime DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bids.Add(new Bid
                    {
                        Amount = reader.GetDecimal(0),
                        BidTime = reader.GetDateTime(1),
                        UserName = reader.GetString(2)
                    });
                }
            }

            return bids;
        }






        public AdBiddingInfo GetBiddingInfo(int adId)
        {
            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = "SELECT * FROM AdBiddingInfo WHERE AdId = @AdId AND IsActive = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@AdId", adId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AdBiddingInfo
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            AdId = Convert.ToInt32(reader["AdId"]),
                            StartTime = Convert.ToDateTime(reader["StartTime"]),
                            EndTime = Convert.ToDateTime(reader["EndTime"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        };
                    }
                }
            }
            return null;
        }

        public List<Ad> GetAdsByUserId(int userId)
        {
            List<Ad> ads = new List<Ad>();

            using (SqlConnection conn = new SqlConnection(constr))
            {
                string query = @"
            SELECT a.*, 
                   (SELECT COUNT(*) FROM Favorites f WHERE f.AdId = a.Id) AS FavoritesCount
            FROM Ads a
            WHERE a.UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Ad ad = new Ad
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                        Category = reader["Category"].ToString(),
                        Price = reader["Price"] == DBNull.Value ? null : (decimal?)reader["Price"],
                        PriceOnCall = Convert.ToBoolean(reader["PriceOnCall"]),
                        Description = reader["Description"].ToString(),
                        ImagePath = reader["ImagePath"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Email = reader["Email"].ToString(),
                        Address = reader["Address"].ToString(),
                        Country = reader["Country"].ToString(),
                        City = reader["City"].ToString(),
                        AgreeToTerms = Convert.ToBoolean(reader["AgreeToTerms"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        Status = reader["Status"].ToString(),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        FavoritesCount = Convert.ToInt32(reader["FavoritesCount"]) // 👈 this is important
                    };

                    ads.Add(ad);
                }

                reader.Close();
            }

            return ads;
        }


        public void AddToFavorites(int userId, int adId)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "IF NOT EXISTS (SELECT 1 FROM Favorites WHERE UserId = @UserId AND AdId = @AdId) " +
                               "INSERT INTO Favorites (UserId, AdId) VALUES (@UserId, @AdId)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@AdId", adId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Ad> GetFavoriteAdsByUser(int userId)
        {
            List<Ad> favoriteAds = new List<Ad>();

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"SELECT A.* 
                         FROM Ads A
                         INNER JOIN Favorites F ON A.Id = F.AdId
                         WHERE F.UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Ad ad = new Ad
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString(),
                        Category = reader["Category"].ToString(),
                        Price = reader["Price"] != DBNull.Value ? (decimal?)reader["Price"] : null,
                        ImagePath = reader["ImagePath"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"],
                        // Add more fields as needed...
                    };

                    favoriteAds.Add(ad);
                }
            }

            return favoriteAds;
        }

        public void UpdateAd(Ad ad)
        {
            string query = @"UPDATE Ads 
                 SET Title = @Title, 
                     Category = @Category, 
                     Price = @Price, 
                     PriceOnCall = @PriceOnCall, 
                     Description = @Description, 
                     FirstName = @FirstName, 
                     LastName = @LastName, 
                     Phone = @Phone, 
                     Email = @Email, 
                     Address = @Address, 
                     Country = @Country, 
                     City = @City, 
                     AgreeToTerms = @AgreeToTerms, 
                     Status = @Status 
                 WHERE Id = @Id";
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Title", ad.Title);
                    cmd.Parameters.AddWithValue("@Category", ad.Category);
                    cmd.Parameters.AddWithValue("@Price", ad.Price ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PriceOnCall", ad.PriceOnCall);
                    cmd.Parameters.AddWithValue("@Description", ad.Description);
                    cmd.Parameters.AddWithValue("@FirstName", ad.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", ad.LastName);
                    cmd.Parameters.AddWithValue("@Phone", ad.Phone);
                    cmd.Parameters.AddWithValue("@Email", ad.Email);
                    cmd.Parameters.AddWithValue("@Address", ad.Address);
                    cmd.Parameters.AddWithValue("@Country", ad.Country);
                    cmd.Parameters.AddWithValue("@City", ad.City);
                    cmd.Parameters.AddWithValue("@AgreeToTerms", ad.AgreeToTerms);
                    cmd.Parameters.AddWithValue("@Status", ad.Status);
                    cmd.Parameters.AddWithValue("@Id", ad.Id);  // Assuming your Ad class also has Id


                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void DeleteFavorite(int userId, int adId)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "DELETE FROM Favorites WHERE UserId = @UserId AND AdId = @AdId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@AdId", adId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }


        public void DeleteAdImage(int adId, string imagePath)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "DELETE FROM AdImages WHERE AdId = @AdId AND ImagePath = @ImagePath";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            // Also delete physical file from server
            string physicalPath = HttpContext.Current.Server.MapPath(imagePath);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        public List<string> GetAdImagePaths(int adId)
        {
            List<string> imagePaths = new List<string>();

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT ImagePath FROM AdImages WHERE AdId = @AdId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AdId", adId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    imagePaths.Add(reader.GetString(0));
                }
            }

            return imagePaths;
        }


        public void AddAdImage(int adId, string imagePath)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "INSERT INTO AdImages (AdId, ImagePath) VALUES (@AdId, @ImagePath)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@AdId", adId);
                cmd.Parameters.AddWithValue("@ImagePath", imagePath);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public void DeleteAd(int id)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();

                // First delete from Favorites
                string deleteFavorites = "DELETE FROM Favorites WHERE AdId = @Id";
                SqlCommand cmd1 = new SqlCommand(deleteFavorites, con);
                cmd1.Parameters.AddWithValue("@Id", id);
                cmd1.ExecuteNonQuery();

                // Then delete from Ads
                string deleteAd = "DELETE FROM Ads WHERE Id = @Id";
                SqlCommand cmd2 = new SqlCommand(deleteAd, con);
                cmd2.Parameters.AddWithValue("@Id", id);
                cmd2.ExecuteNonQuery();
            }
        }



        public List<Ad> SearchAds(string query)
        {
            List<Ad> ads = new List<Ad>();

            con.Open();
            SqlCommand cmd = new SqlCommand(@"SELECT Id, Title, Description, ImagePath, Category, Address, Price, PriceOnCall 
                                      FROM Ads 
                                      WHERE Title LIKE @Query 
                                         OR Description LIKE @Query 
                                         OR Category LIKE @Query", con);
            cmd.Parameters.AddWithValue("@Query", "%" + query + "%");

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Ad ad = new Ad
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    ImagePath = reader["ImagePath"].ToString(),
                    Category = reader["Category"].ToString(),
                    Address = reader["Address"].ToString(),
                    Price = Convert.ToInt32(reader["Price"]),
                    PriceOnCall = Convert.ToBoolean(reader["PriceOnCall"])
                };
                ads.Add(ad);
            }

            reader.Close();
            con.Close();

            return ads;
        }



        public List<Ad> GetAdsByCategory(string category)
        {
            List<Ad> ads = new List<Ad>();

            using (SqlConnection con = new SqlConnection(constr))
            {
                con.Open();
                string query = "SELECT * FROM Ads WHERE Category = @Category";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Category", category);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Ad ad = new Ad
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Category = reader["Category"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                PriceOnCall = Convert.ToBoolean(reader["PriceOnCall"]),
                                ImagePath = reader["ImagePath"].ToString(),
                                Address = reader["Address"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                // Agar aur columns hain to wo bhi add karlo
                            };
                            ads.Add(ad);
                        }
                    }
                }
            }

            return ads;
        }



        public void InsertUpdateDelete(string query)
        {
            cmd = new SqlCommand(query, con);
            cmd.ExecuteNonQuery();
        }

        //below function Return the Reader
        public SqlDataReader getData(string query)
        {
            cmd = new SqlCommand(query, con);
            sdr = cmd.ExecuteReader();
            return sdr;
        }
    }
}



