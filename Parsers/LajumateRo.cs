using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PostgreSQL;

// using System.Net.Http.Headers;
// using Newtonsoft.Json;
// using System.Text;
namespace Parser
{
    public class LajumateRo
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static char[] MyChar = {';',')', '[', ']'};
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36";
        private static string cookie = "XSRF-TOKEN=eyJpdiI6Im5aWENzcWVYMG9rNVRBbWNsNHRGXC9BPT0iLCJ2YWx1ZSI6IlpWNWxjaUNDaTVqQlV0cUtWaWl1OUdwbjFoK2JnTmY1dGhtbVhFQmQ1R1RDajVkU0tvQXpDWVNVWFJNaGlUQng3WlJrdmwzZmdvVXZpTjR4ZmMzTEhBPT0iLCJtYWMiOiI4NmY2NzQ0MDBhYzkxOTZkOTIxYWIzMWE4YjBlMDdmZTJjY2QwN2JjOTZjNjQzYTY2YzgwNDRjYTQzNjFkNGQ3In0%3D; ljs=eyJpdiI6ImN2ZnZpc29FZDN3bkY2aWVhT1E4anc9PSIsInZhbHVlIjoiXC9BXC9PQ1lxZjJxTU9mdFpaaU1mbVFFSTRSU3Roa2VhY09pZE4rM3lOQW1SZUQrTTM1eGV1aWh3Nm1nZlBRTksxWHh1cU9rYWowNXhCTzM0T0xGbVp3UT09IiwibWFjIjoiMjU4NTBiMzI4ZmZiYWM0ZDA3MmQyY2M5MjU4NTk0MGE1MzkzMDljZTE1OWUwNmEyYThkMDJiYTdiMGI0ZGEwNyJ9; SRV=s3";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static HtmlWeb web = new HtmlWeb();
        
        public static int StartParsing(long userId)
        {
            annoounCount = 0;
            int pagesPassed = 1;
            int adsPassed = 0;
            int errorRequests = 0;

            try
            {   
                userPlatform = DB.GetPlatform(userId);
                string userLink = DB.GetLink(userId);
                int userAnnounCount = DB.GetAnnounCount(userId);
                string userSellerAdCount = DB.GetSellerAdvCount(userId);
                string userSellerRegDate = DB.GetSellerRegData(userId);
                string userAdRegDate = DB.GetAdvRegData(userId);
                decimal userSellerRating = DB.GetSellerRating(userId);
                string userSellerType = DB.GetSellerType(userId);
                int page = DB.GetStartPage(userId);
                string localBlacklist = DB.GetLocalAndGlobalBl(userId)[0];
                string globalBlacklist = DB.GetLocalAndGlobalBl(userId)[1];


                while(true)
                {
                    web.UserAgent = userAgent;
                    HtmlDocument document = web.Load(GenerateLink(userId, userLink, page));
                    var advertisements = document.DocumentNode.SelectNodes("//a[contains(@class, 'main_items item_cart')]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string adLink = advertisement.GetAttributeValue("href", "");

                                adsPassed++;
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);

                                if(!DB.CheckAdvestisement(userId, adLink))
                                {
                                    ParsAdInfo(userId, adLink, userSellerAdCount, userSellerRegDate, userAdRegDate, userSellerType, userSellerRating, localBlacklist, globalBlacklist);
                                }
                                continue;
                            }
                            else
                            {
                                string state = "StopParser";
                                DB.UpdateState(userId, state);
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        if(errorRequests < 3)
                        {
                            System.Threading.Thread.Sleep(2000);
                            errorRequests++;
                            continue;
                        }
                        else
                        {
                            string state = "StopParser";
                            DB.UpdateState(userId, state);
                            DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                            return 1;
                        }
                    }
                    pagesPassed++;
                    page++;
                }
            }
            catch(Exception)
            {
                string state = "StopParser";
                DB.UpdateState(userId, state);
                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                return 1;
            }

        }

        static void ParsAdInfo(long userId, string adLink, string userSellerAdCount, string userSellerRegDate, string userAdRegDate, string userSellerType, decimal userSellerRating, string localBlacklist, string globalBlacklist)
        {
            string adTitle = "Не указано";
            string adPrice = "Не указана";
            string adImage = errorImageUri;
            string sellerType = "Частное лицо";
            string sellerName = "Не указано";
            string sellerLink = "Не указана";
            string adLocation = "Не указано";
            string sellerPhoneNumber = "";
            int sellerTotalAds = 1;
            decimal sellerRating = 0.0M;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;
            

            var adDocument = new HtmlDocument();
            adDocument.LoadHtml(GetPageInfo(adLink).Result);

            try
            {
                
                string _token = adDocument.DocumentNode.SelectSingleNode($"//input[@name=\"_token\"]").GetAttributeValue("value", "");

                string _adId = adDocument.DocumentNode.SelectSingleNode($"//input[@name=\"ad_id\"]").GetAttributeValue("value", "");

                string _encrypted = adDocument.DocumentNode.SelectSingleNode($"//input[@name=\"encrypted\"]").GetAttributeValue("value", "");
                sellerPhoneNumber = GetPhoneNumber(_token, _adId, _encrypted, adLink).Result;

                if(sellerPhoneNumber == "null")
                {
                    return;
                }
            }
            catch{ return; }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ return; }

            try
            {
                var sellerTypeBlock = adDocument.DocumentNode.SelectSingleNode($"//span[@class=\"rounded\"]").InnerText;

                if(sellerTypeBlock.Contains("Persoană fizică"))
                {
                    sellerType = "Частное лицо";
                }
                else
                {
                    sellerType = "Бизнесс аккаунт";
                }
            }
            catch{}
            
            if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }
            
            try
            {
                adRegDate = ConvertAdRegDate(adDocument.DocumentNode.SelectSingleNode("//span[@id=\"date\"]").InnerText);
            }
            catch{}

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }

            try
            {
                sellerName = adDocument.DocumentNode.SelectSingleNode($"//div[@id=\"account_holder\"]//a[@class=\"holder_top_link\"]").GetAttributeValue("title", "");
                sellerLink = adDocument.DocumentNode.SelectSingleNode($"//div[@id=\"account_holder\"]//a[@class=\"holder_top_link\"]").GetAttributeValue("href", "");
            }
            catch{ return; }

            HtmlDocument sellerDocument = web.Load(sellerLink);

            try
            {
                sellerRegDate = Convert.ToDateTime(sellerDocument.DocumentNode.SelectNodes($"//div[@id=\"user_ads_sidebar\"]//div[@class=\"user_on_site\"]//span")[1].InnerText);
            }
            catch{}

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                sellerTotalAds = Int32.Parse(sellerDocument.DocumentNode.SelectSingleNode($"//div[@id=\"user_ads_sidebar\"]//div[@class=\"sidebar_info\"]//strong").InnerText);
            }
            catch{}

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = adDocument.DocumentNode.SelectSingleNode("//h1[@itemprop=\"name\"]").InnerText;
            }
            catch{}

            try
            {
                adImage = adDocument.DocumentNode.SelectSingleNode("//img[@itemprop=\"representativeOfPage\"]").GetAttributeValue("src", "");
            }
            catch{}

            try
            {
                adLocation = adDocument.DocumentNode.SelectSingleNode("//span[@id=\"location_city\"]").InnerText.Trim().Replace(" ", "").Replace("\n", "").Replace(",", ", ");
            }
            catch{}
            
            try
            {
                string adAmount = adDocument.DocumentNode.SelectSingleNode("//span[@itemprop=\"price\"]").InnerText;
                string adCurrency = "LEI";
                adPrice = Functions.convert_price(adAmount, adCurrency);
            }
            catch{}


            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
            
        }

        static async Task<String> GetPhoneNumber(string _token, string _adId, string _encrypted, string adLink)
        {
            try
            {
                var url = "https://lajumate.ro/ajax/phone-number";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                client.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/x-www-form-urlencode");
                client.DefaultRequestHeaders.Add("cookie", cookie);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);

                var data = new Dictionary<string, string>
                {
                    { "_token", _token},
                    { "ad_id", _adId },
                    { "encrypted", _encrypted },
                };

                var request = await client.PostAsync(url, new FormUrlEncodedContent(data));
                var content = await request.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(content);
                string phoneNumber = Functions.convert_phone(jObject["phone"]!.ToString());
                

                if(!phoneNumber.Contains("+"))
                {
                    phoneNumber = $"+4{phoneNumber}";
                }

                return phoneNumber;
            }
            catch
            {
                return "null";
            }
        }

        async static Task<string> GetPageInfo(string adLink)
        {
            
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            client.DefaultRequestHeaders.Add("cookie", cookie);

            HttpResponseMessage response = await client.GetAsync(adLink);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            return content;
        } 


        static string GenerateLink(long userId, string link, int page)
        {
            string pageLink = "";

            if(link.Contains("https://lajumate.ro/"))
            {
                pageLink = link.Split(".html")[0] + "_" + page.ToString() + ".html";
            }
            else
            {
                pageLink = $"https://lajumate.ro/cauta_{link}_{page}.html";
            }
            return pageLink;
        }


        static DateTime ConvertAdRegDate(string dateBlock)
        {
            DateTime date;
            string dateNumber = "";
            for (int i = 0; i < dateBlock.Length; i++)
            {
                if(Char.IsDigit(dateBlock[i]))
                {
                    dateNumber += dateBlock[i]; 
                }
                continue;
            }
            if(dateNumber == "")
            {
                dateNumber = "1";
            }

            if(dateBlock.Contains("minut"))
            {
                date = DateTime.Now;
            }
            else if(dateBlock.Contains("or"))
            {
                int hours = Int32.Parse(dateNumber);
                date = DateTime.Now.AddHours(-hours);
            }
            else if(dateBlock.Contains("zi"))
            {
                int days = Int32.Parse(dateNumber);
                date = DateTime.Now.AddDays(-days);
            }
            else if(dateBlock.Contains("săptăm"))
            {
                int weeks = Int32.Parse(dateNumber);
                date = DateTime.Now.AddDays(-weeks*7);
            }
            else
            {
                date = DateTime.Now;
            }

            return date;
        }
    }
}