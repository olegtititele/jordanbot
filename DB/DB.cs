using Npgsql;

using Bot_Keyboards;
using Telegram.Bot.Types.ReplyMarkups;
namespace PostgreSQL
{
    public static class DB
    {
        private static string db_connection = DBConfig.db;

        public static List<long> GetAllUsersId()
        {
            List<long> usersID= new List<long>();
            using var con = new NpgsqlConnection(db_connection);
            con.Open();


            var sql = $"SELECT userid FROM users_table";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    usersID.Add(reader.GetInt64(0));
                }
                con.Close();
                return usersID;
            }
        }

        public static void UpdateStates()
        {
            foreach(var userId in GetAllUsersId())
            {
                try
                {
                    if(GetState(userId)=="Parser" || GetState(userId)=="StopParser")
                    {
                        UpdateState(userId, "MainMenu");
                    }
                }
                catch(Exception){ }
            }
        }

        // User

        public static void CreateUsersTable()
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"CREATE TABLE IF NOT EXISTS users_table (UserId BIGINT PRIMARY KEY, UserName TEXT, StartPage INT, ShowBusinessAcc TEXT, WhatsappText TEXT, State TEXT, Link TEXT, Announ_count TEXT, Seller_Adv TEXT, Seller_Reg_Data TEXT, Adv_Reg_Data TEXT, Platform TEXT, local_blacklist TEXT, global_blacklist TEXT, pages_passed INT, ads_passed INT, token TEXT, seller_rating REAL)";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static void AddColumn()
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            double sellerRating = 5;
            
            cmd.CommandText = $"ALTER TABLE users_table ADD seller_rating REAL NOT NULL DEFAULT {sellerRating};";
            cmd.ExecuteNonQuery();

            foreach(long userId in DB.GetAllUsersId())
            {
                Console.WriteLine(userId);
                cmd.CommandText = $"ALTER TABLE a{userId} ADD seller_rating TEXT NOT NULL DEFAULT '{sellerRating}';";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"ALTER TABLE h{userId} ADD seller_rating TEXT NOT NULL DEFAULT '{sellerRating}';";
                cmd.ExecuteNonQuery();
            }
            con.Close();
        }

        public static void CreateUser(long userId, string username)
        {
            string whatsappText = "Hello, I want to buy this. In a good condition? @adlink";
            int startPage = 1;
            string showTypeAcc = "Частное лицо";
            string state = "MainMenu";
            string link = "null";
            string platform = "null";
            string announCount = "10";
            string sellerAdv = "Отключить";
            string sellerRegData = "Отключить";
            string advRegData = "Отключить";
            string localBlacklist = "Включить";
            string globalBlacklist = "Отключить";
            int pagesPassed = 0;
            int adsPassed = 0;
            string token = "Не указан";
            double sellerRating = 5;
            
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = $"INSERT INTO users_table (UserId, UserName, StartPage, ShowBusinessAcc, WhatsappText, State, Link, Announ_count, Seller_Adv, Seller_Reg_Data, Adv_Reg_Data, Platform, local_blacklist, global_blacklist, pages_passed, ads_passed, token, seller_rating) VALUES (@userid, @username, @startpage, @showbusinessacc, @whatsapptext, @state, @link, @announ_count, @seller_adv, @seller_reg_data, @adv_reg_data, @platform, @local_blacklist, @global_blacklist, @pages_passed, @ads_passed, @token, @seller_rating)";
            cmd.Parameters.AddWithValue("@userid", userId);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@startpage", startPage);
            cmd.Parameters.AddWithValue("@showbusinessacc", showTypeAcc);
            cmd.Parameters.AddWithValue("@whatsapptext", whatsappText);
            cmd.Parameters.AddWithValue("@state", state);
            cmd.Parameters.AddWithValue("@link", link);
            cmd.Parameters.AddWithValue("@announ_count", announCount);
            cmd.Parameters.AddWithValue("@seller_adv", sellerAdv);
            cmd.Parameters.AddWithValue("@seller_reg_data", sellerRegData);
            cmd.Parameters.AddWithValue("@adv_reg_data", advRegData);
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@local_blacklist", localBlacklist);
            cmd.Parameters.AddWithValue("@global_blacklist", globalBlacklist);
            cmd.Parameters.AddWithValue("@pages_passed", pagesPassed);
            cmd.Parameters.AddWithValue("@ads_passed", adsPassed);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@seller_rating", sellerRating);
            cmd.ExecuteNonQuery();
            
        }

        public static bool CheckUser(long user_id)
        {
            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM users_table WHERE UserId = '{user_id}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetState(long user_id)
        {
            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT state FROM users_table WHERE UserId = '{user_id}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar();
                return result!.ToString()!;
            }
            catch(Exception)
            {
                return "MainMenu";
            }
        }
        public static void UpdateState(long user_id, string state)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET state = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", state);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }
        

        public static string GetPlatform(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT Platform FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static string GetLink(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT Link FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateStatistic(long user_id, int pages_passed, int ads_passed)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET pages_passed = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", pages_passed);
                    int nRows = command.ExecuteNonQuery();
                }
                using (var command = new NpgsqlCommand("UPDATE users_table SET ads_passed = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", ads_passed);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetStatistic(long user_id)
        {
            List<string> parameters= new List<string>();
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT pages_passed, ads_passed FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    parameters.Add(reader.GetInt32(0).ToString());
                    parameters.Add(reader.GetInt32(1).ToString());
                }
                con.Close();
                return parameters;
            }    
        }

        public static List<string> GetAllParameters(long user_id)
        {
            List<string> parameters= new List<string>();
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT platform, link, announ_count, seller_adv, seller_reg_data, adv_reg_data, showbusinessacc, startpage, local_blacklist, global_blacklist, token, seller_rating FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    parameters.Add(reader.GetString(0));
                    parameters.Add(reader.GetString(1));
                    parameters.Add(reader.GetString(2));
                    parameters.Add(reader.GetString(3));
                    parameters.Add(reader.GetString(4));
                    parameters.Add(reader.GetString(5));
                    parameters.Add(reader.GetString(6));
                    parameters.Add(reader.GetInt32(7).ToString());
                    parameters.Add(reader.GetString(8));
                    parameters.Add(reader.GetString(9));
                    parameters.Add(reader.GetString(10));
                    parameters.Add(reader.GetDouble(11).ToString());
                }
                con.Close();
                return parameters;
            }    
        }

        public static void UpdatePlatform(long user_id, string platform)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET platform = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", platform);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }

        public static void UpdateLink(long userId, string link)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET link = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", link);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }

        public static void UpdateAnnounCount(long userId, string announCount)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET announ_count = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", announCount);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }

        public static int GetAnnounCount(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT announ_count FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return Int32.Parse(result!.ToString()!);
        }

        public static decimal GetSellerRating(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT seller_rating FROM users_table WHERE userid = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return decimal.Parse(result!.ToString()!);
        }

        public static string GetSellerType(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT showbusinessacc FROM users_table WHERE userid = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateSellerRating(long userId, double sellerRating)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET seller_rating = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", sellerRating);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateSellerAdv(long userId, string sellerAdv)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET seller_adv = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", sellerAdv);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }
        public static string GetSellerAdvCount(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT seller_adv FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateSellerRegData(long userId, string sellerRegData)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET seller_reg_data = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", sellerRegData);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }
        public static string GetSellerRegData(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT seller_reg_data FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateAdvRegData(long userId, string advRegData)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET adv_reg_data = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", userId);
                    command.Parameters.AddWithValue("q", advRegData);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }
        public static string GetAdvRegData(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT adv_reg_data FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static string ShowAccType(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT ShowBusinessAcc FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateAccType(long user_id)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                conn.Open();

                if(ShowAccType(user_id) == "Частное лицо")
                {
                    using (var command = new NpgsqlCommand("UPDATE users_table SET showbusinessacc = @q WHERE userid = @n", conn))
                    {
                        command.Parameters.AddWithValue("n", user_id);
                        command.Parameters.AddWithValue("q", "Все типы аккаунтов");
                        int nRows = command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var command = new NpgsqlCommand("UPDATE users_table SET showbusinessacc = @q WHERE userid = @n", conn))
                    {
                        command.Parameters.AddWithValue("n", user_id);
                        command.Parameters.AddWithValue("q", "Частное лицо");
                        int nRows = command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static string GetWhatsappText(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            
            var sql = $"SELECT whatsapptext FROM users_table WHERE userid = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            if(result == null)
            {
                return "";
            }
            return result!.ToString()!;
                
        }
        
        public static void UpdateWhatsappText(long user_id, string whatsapp_text)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET whatsapptext = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", whatsapp_text);
                    int nRows = command.ExecuteNonQuery();
                    
                }
            }
        }

        public static int GetStartPage(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT startpage FROM users_table WHERE userid = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        public static void UpdateStartPage(long user_id, int start_page)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET startpage = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", start_page);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static string GetToken(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            var sql = $"SELECT token FROM users_table WHERE userid = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static void UpdateToken(long user_id, string token)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET token = @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", token);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static List<string> GetLocalAndGlobalBl(long user_id)
        {
            List<string> parameters= new List<string>();
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT local_blacklist, global_blacklist FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    parameters.Add(reader.GetString(0));
                    parameters.Add(reader.GetString(1));
                }
                con.Close();
                return parameters;
            }    
        }
        public static string GetUsersDbLenght()
        {
            int usersCount = 0;
            foreach(var user in DB.GetAllUsersId())
            {
                usersCount++;
            }
            return usersCount.ToString();
        }

        public static string ShowUsername(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();


            var sql = $"SELECT username FROM users_table WHERE UserId = '{user_id}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }

        public static string ShowUserId(string username)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();


            var sql = $"SELECT userid FROM users_table WHERE username = '{username}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var result = cmd.ExecuteScalar();
            return result!.ToString()!;
        }


        // BlackListDB

        public static void CreateAdsBlacklistTable()
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"CREATE TABLE IF NOT EXISTS sellers_blacklist (Platform TEXT, SellerLink TEXT, SellerPhone TEXT)";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static void UpdateLocalBlackList(long user_id, string local_blacklist)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET local_blacklist= @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", local_blacklist);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateGlobalBlackList(long user_id, string global_blacklist)
        {
            using (var conn = new NpgsqlConnection(db_connection))
            {
                conn.Open();

                using (var command = new NpgsqlCommand("UPDATE users_table SET global_blacklist= @q WHERE userid = @n", conn))
                {
                    command.Parameters.AddWithValue("n", user_id);
                    command.Parameters.AddWithValue("q", global_blacklist);
                    int nRows = command.ExecuteNonQuery();
                }
            }
        }

        public static long GlobalBlacklistLength()
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT * FROM sellers_blacklist";
            using var cmd = new NpgsqlCommand(sql, con);
            var reader = cmd.ExecuteReader();
            int length = 0;
            while (reader.Read())
            {
                length++;
            }
            return length;
        }

        public static bool CheckBlackListSellerPhone(long user_id, string sellerphone)
        {

            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM sellers_blacklist WHERE sellerphone = '{sellerphone}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckBlackListSellerLink(long user_id, string sellerlink)
        {

            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM sellers_blacklist WHERE sellerlink = '{sellerlink}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void AddAdvertisemtToBlackList(string platform, string seller_link, string seller_phone)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"INSERT INTO sellers_blacklist (Platform, SellerLink, SellerPhone) VALUES (@platform, @seller_link, @seller_phone)";
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@seller_link", seller_link);
            cmd.Parameters.AddWithValue("@seller_phone", seller_phone);
            cmd.ExecuteNonQuery();
        }

        // MainStorage

        public static void CreateAdvertisementTable(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"CREATE TABLE IF NOT EXISTS a{user_id} (AdPlatform TEXT, AdvertisementName TEXT, AdvertisementPrice TEXT, AdvertisementRegDate TEXT, AdvertisementLink TEXT, Location TEXT, ImageLink TEXT, SellerName TEXT, SellerLink TEXT, SellerPhone TEXT, SellerAdvCount TEXT, SellerRegDate TEXT, BusinessAcc TEXT, seller_rating TEXT)";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static string LocalBlacklistLength(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT DISTINCT sellerphone FROM a{user_id}";
            using var cmd = new NpgsqlCommand(sql, con);
            var reader = cmd.ExecuteReader();
            int length = 0;
            while (reader.Read())
            {
                length++;
            }
            return length.ToString();
        }

        // HashDB

        public static void CreateHashTable(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"CREATE TABLE IF NOT EXISTS h{user_id} (AdPlatform TEXT, AdvertisementName TEXT, AdvertisementPrice TEXT, AdvertisementRegDate TEXT, AdvertisementLink TEXT, Location TEXT, ImageLink TEXT, SellerName TEXT, SellerLink TEXT, SellerPhone TEXT, SellerAdvCount TEXT, SellerRegDate TEXT, BusinessAcc TEXT, seller_rating TEXT)";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static List<List<string>> GetHashData(long user_id)
        {
            List<List<string>> all_hash_data= new List<List<string>>();
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT * FROM h{user_id}";
            using var cmd = new NpgsqlCommand(sql, con);
            cmd.ExecuteNonQuery();
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> list= new List<string>();
                    list.Add(reader.GetString(0));
                    list.Add(reader.GetString(1));
                    list.Add(reader.GetString(2));
                    list.Add(reader.GetString(3));
                    list.Add(reader.GetString(4));
                    list.Add(reader.GetString(5));
                    list.Add(reader.GetString(6));
                    list.Add(reader.GetString(7));
                    list.Add(reader.GetString(8));
                    list.Add(reader.GetString(9));
                    list.Add(reader.GetString(10));
                    list.Add(reader.GetString(11));
                    list.Add(reader.GetString(12));
                    list.Add(reader.GetString(13));
                    all_hash_data.Add(list);
                }
                con.Close();
                return all_hash_data;
            }    
        }

        public static int LengthHashData(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();
            var sql = $"SELECT * FROM h{user_id}";
            using var cmd = new NpgsqlCommand(sql, con);
            var reader = cmd.ExecuteReader();
            int length = 0;
            while (reader.Read())
            {
                length++;
            }
            return length;
        }

        public static int LengthAdvsWithoutNumber(long user_id)
        {
            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM h{user_id} WHERE sellerphone='Не указан'";
                using var cmd = new NpgsqlCommand(sql, con);
                var reader = cmd.ExecuteReader();
                int length = 0;
                while (reader.Read())
                {
                    length++;
                }
                return length;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static void ClearHash(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = $"TRUNCATE TABLE h{user_id}";
            cmd.ExecuteNonQuery();
        }

        // ParserDB

        public static void ClearMain(long user_id)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = $"TRUNCATE TABLE a{user_id}";
            cmd.ExecuteNonQuery();
        }
        

        public static bool CheckAdvestisement(long user_id, string adv_url)
        {

            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM a{user_id} WHERE AdvertisementLink = '{adv_url}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool CheckPhoneNumber(long user_id, string seller_phone)
        {

            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                var sql = $"SELECT * FROM a{user_id} WHERE SellerPhone = '{seller_phone}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool CheckSellerlink(long user_id, string seller_link)
        {
            try
            {
                using var con = new NpgsqlConnection(db_connection);
                con.Open();
                
                var sql = $"SELECT * FROM a{user_id} WHERE SellerLink = '{seller_link}'";
                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()!.ToString();
                con.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static void AddAdvertisementToHash(long userId, string platform, string adTitle, string adPrice, string adRegDate, string adLink, string adLocation, string adImage, string sellerName, string sellerLink, string phoneNumber, string sellerTotalAds, string sellerRegDate, string sellerType, string sellerRating)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;

            cmd.CommandText = $"INSERT INTO h{userId} (AdPlatform, AdvertisementName, AdvertisementPrice, AdvertisementRegDate, AdvertisementLink, Location, ImageLink, SellerName, SellerLink, SellerPhone, SellerAdvCount, SellerRegDate, BusinessAcc, seller_rating) VALUES (@platform, @adv_title, @adv_price, @adv_reg, @adv_link, @location, @adv_image, @seller_name, @seller_link, @seller_phone, @seller_adv_count, @seller_reg_date, @adv_business, @seller_rating)";
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@adv_title", adTitle);
            cmd.Parameters.AddWithValue("@adv_price", adPrice);
            cmd.Parameters.AddWithValue("@adv_reg", adRegDate);
            cmd.Parameters.AddWithValue("@adv_link", adLink);
            cmd.Parameters.AddWithValue("@location", adLocation);
            cmd.Parameters.AddWithValue("@adv_image", adImage);
            cmd.Parameters.AddWithValue("@seller_name", sellerName);
            cmd.Parameters.AddWithValue("@seller_link", sellerLink);
            cmd.Parameters.AddWithValue("@seller_phone", phoneNumber);
            cmd.Parameters.AddWithValue("@seller_adv_count", sellerTotalAds);
            cmd.Parameters.AddWithValue("@seller_reg_date", sellerRegDate);
            cmd.Parameters.AddWithValue("@adv_business", sellerType);
            cmd.Parameters.AddWithValue("@seller_rating", sellerRating);
            cmd.ExecuteNonQuery();
        }

        public static void AddAdvertisementToMain(long userId, string platform, string adTitle, string adPrice, string adRegDate, string adLink, string adLocation, string adImage, string sellerName, string sellerLink, string phoneNumber, string sellerTotalAds, string sellerRegDate, string sellerType, string sellerRating)
        {
            using var con = new NpgsqlConnection(db_connection);
            con.Open();

            using var cmd = new NpgsqlCommand();
            cmd.Connection = con;
            cmd.CommandText = $"INSERT INTO a{userId} (AdPlatform, AdvertisementName, AdvertisementPrice, AdvertisementRegDate, AdvertisementLink, Location, ImageLink, SellerName, SellerLink, SellerPhone, SellerAdvCount, SellerRegDate, BusinessAcc, seller_rating) VALUES (@platform, @adv_title, @adv_price, @adv_reg, @adv_link, @location, @adv_image, @seller_name, @seller_link, @seller_phone, @seller_adv_count, @seller_reg_date, @adv_business, @seller_rating)";
            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@adv_title", adTitle);
            cmd.Parameters.AddWithValue("@adv_price", adPrice);
            cmd.Parameters.AddWithValue("@adv_reg", adRegDate);
            cmd.Parameters.AddWithValue("@adv_link", adLink);
            cmd.Parameters.AddWithValue("@location", adLocation);
            cmd.Parameters.AddWithValue("@adv_image", adImage);
            cmd.Parameters.AddWithValue("@seller_name", sellerName);
            cmd.Parameters.AddWithValue("@seller_link", sellerLink);
            cmd.Parameters.AddWithValue("@seller_phone", phoneNumber);
            cmd.Parameters.AddWithValue("@seller_adv_count", sellerTotalAds);
            cmd.Parameters.AddWithValue("@seller_reg_date", sellerRegDate);
            cmd.Parameters.AddWithValue("@adv_business", sellerType);
            cmd.Parameters.AddWithValue("@seller_rating", sellerRating);
            cmd.ExecuteNonQuery();
        }
    }
}