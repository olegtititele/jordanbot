using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class OLX
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static HtmlWeb web = new HtmlWeb();
        
        public static void StartParsing(long userId)
        {
            annoounCount = 0;
            int pagesPassed = 1;
            int adsPassed = 0;

            try
            {    
                List<string> parameters = DB.GetAllParameters(userId);
                userPlatform = parameters[0];
                string userLink = parameters[1];
                int userAnnounCount = Int32.Parse(parameters[2]);
                string userSellerAdCount = parameters[3];
                string userSellerRegDate = parameters[4];
                string userAdvRegDate = parameters[5];
                string userSellerType = parameters[6];
                int page = Int32.Parse(parameters[7]);
                string localBlacklist = parameters[8];
                string globalBlacklist = parameters[9];

                while(true)
                {
                    web.UserAgent = userAgent;
                    HtmlDocument document = web.Load(GenerateLink(userLink, page));
                    Console.WriteLine(GenerateLink(userLink, page));
                    
                    HtmlNodeCollection advertisements = document.DocumentNode.SelectNodes("//div[@data-cy=\"l-card\"]//a[@class=\"css-1bbgabe\"]");
                    if(advertisements == null)
                    {
                        advertisements = document.DocumentNode.SelectNodes("//a[@data-cy=\"listing-ad-title\"]");
                    }
                    
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string adLink = "";
                                string listingAd = advertisement.GetAttributeValue("href", "").Split("#")[0];

                                switch(userPlatform)
                                {
                                    case "olx.ro":
                                        if(listingAd.Contains("https://"))
                                        {
                                            adLink = listingAd;
                                        }
                                        else
                                        {
                                            adLink = "https://www.olx.ro" + listingAd;
                                        }
                                        break;
                                    case "olx.pt":
                                        if(listingAd.Contains("https://"))
                                        {
                                            adLink = listingAd;
                                        }
                                        else
                                        {
                                            adLink = "https://www.olx.pt" + listingAd;
                                        }
                                        break;
                                        
                                    case "olx.pl":
                                        if(listingAd.Contains("https://"))
                                        {
                                            adLink = listingAd;
                                        }
                                        else
                                        {
                                            adLink = "https://www.olx.pl" + listingAd;
                                        }
                                        break;
                                        
                                    case "olx.bg":
                                        if(listingAd.Contains("https://"))
                                        {
                                            adLink = listingAd;
                                        }
                                        else
                                        {
                                            adLink = "https://www.olx.bg" + listingAd;
                                        }
                                        break;
                                        
                                }
                                Console.WriteLine(adLink);
                                adsPassed++;
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);

                                if(!DB.CheckAdvestisement(userId, adLink))
                                {
                                    HtmlDocument adDocument = web.Load(adLink);

                                    string adId;
                                    string sellerPhoneNumber;
                                    string sellerLink = "";
                                    string script = adDocument.DocumentNode.SelectSingleNode("//script[@id=\"olx-init-config\"]").InnerText.Trim();
                                    string json = script.Split("window.__PRERENDERED_STATE__= ")[1].Split('\n')[0].Trim().Replace("\\\"", "\"").Replace("\\\\", "\\");
                                    char[] MyChar = {';', '"'};
                                    json = json.Trim(MyChar);
                                    
                                    JObject jObject = JObject.Parse(json);

                                    switch(userPlatform)
                                    {
                                        case "olx.ro":
                                            sellerLink = "https://www.olx.ro" + adDocument.DocumentNode.SelectSingleNode("//a[@name=\"user_ads\"]").GetAttributeValue("href", "");
                                            break;
                                        case "olx.pt":
                                            sellerLink = "https://www.olx.pt" + adDocument.DocumentNode.SelectSingleNode("//a[@name=\"user_ads\"]").GetAttributeValue("href", "");
                                            break;
                                        case "olx.pl":
                                            sellerLink = "https://www.olx.pl" + adDocument.DocumentNode.SelectSingleNode("//a[@name=\"user_ads\"]").GetAttributeValue("href", "");
                                            break;
                                        case "olx.bg":
                                            sellerLink = "https://www.olx.bg" + adDocument.DocumentNode.SelectSingleNode("//a[@name=\"user_ads\"]").GetAttributeValue("href", "");
                                            break;
                                    }
                                    Console.WriteLine(sellerLink);
                                    if(Functions.CheckIfSellerLinkNotExists(userId, sellerLink, globalBlacklist, localBlacklist))
                                    {
                                        bool ifAdContainsPhone = Convert.ToBoolean(jObject["ad"]!["ad"]!["contact"]!["phone"]!);

                                        if(!ifAdContainsPhone)
                                        {
                                            continue;
                                        }

                                        adId = jObject["ad"]!["ad"]!["id"]!.ToString();

                                        sellerPhoneNumber = GetPhoneNumber(userId, adId).Result;

                                        if(sellerPhoneNumber != "null")
                                        {
                                            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist))
                                            {
                                                ParsAdInfo(userId, adDocument, adLink, sellerPhoneNumber, sellerLink, userSellerAdCount, userSellerRegDate, userAdvRegDate, userSellerType, localBlacklist, globalBlacklist);
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                            else if(DB.GetState(userId)=="ChangeToken")
                            {
                                string state = "ChangeToken";
                                DB.UpdateState(userId, state);
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                                return;
                            }
                            else
                            {
                                string state = "StopParser";
                                DB.UpdateState(userId, state);
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                                return;
                            }
                        }
                    }
                    else
                    {                        
                        string state = "StopParser";
                        DB.UpdateState(userId, state);
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                        return;
                    }
                    pagesPassed++;
                    page++;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                string state = "StopParser";
                DB.UpdateState(userId, state);
                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                return;
            }

        }

        static void ParsAdInfo(long userId, HtmlDocument adDocument, string adLink, string sellerPhoneNumber, string sellerLink, string userSellerAdCount, string userSellerRegDate, string userAdvRegDate, string userSellerType, string localBlacklist, string globalBlacklist)
        {
            string adPrice = "";
            string adTitle = "Не указано";
            string adImage = "";
            string sellerType = "";
            string sellerName = "Не указано";
            string adLocation = "Не указано";
            int sellerTotalAds = 1;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;


            string adScript = adDocument.DocumentNode.SelectSingleNode("//script[@id=\"olx-init-config\"]").InnerText.Trim();
            string adJson = adScript.Split("window.__PRERENDERED_STATE__= ")[1].Split('\n')[0].Trim().Replace("\\\"", "\"").Replace("\\\\", "\\");
            char[] MyChar = {';', '"'};
            adJson = adJson.Trim(MyChar);
            
            JObject jObject = JObject.Parse(adJson);

            try
            {
                adRegDate = Convert.ToDateTime(jObject["ad"]!["ad"]!["createdTime"]!);
            }
            catch(Exception){ }

            if(Functions.check_adv_reg_data(userAdvRegDate, adRegDate)){ }else{ return; }

            try
            {
                sellerRegDate = Convert.ToDateTime(jObject["ad"]!["ad"]!["user"]!["created"]!);
            }
            catch(Exception){ }

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                bool isBusiness = Convert.ToBoolean(jObject["ad"]!["ad"]!["isBusiness"]!);
                if(isBusiness)
                {
                    sellerType = "Бизнесс аккаунт";
                }
                else
                {
                    sellerType = "Частное лицо";
                }
            }
            catch(Exception)
            {
                sellerType = "Частное лицо";
            }

            if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }

            try
            {   
                HtmlDocument sellerDocument = web.Load(sellerLink);
                string sellerScript = sellerDocument.DocumentNode.SelectSingleNode("//script[@id=\"olx-init-config\"]").InnerText.Trim();
                string sellerJson = sellerScript.Split("window.__PRERENDERED_STATE__= ")[1].Split('\n')[0].Trim().Replace("\\\"", "\"").Replace("\\\\", "\\");
                sellerJson = sellerJson.Trim(MyChar);
                JObject sellerJObject = JObject.Parse(sellerJson);
                sellerTotalAds = Int32.Parse(sellerJObject["userListing"]!["userListing"]!["totalElements"]!.ToString());

            }
            catch(Exception){ }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = jObject["ad"]!["ad"]!["title"]!.ToString();
            }
            catch(Exception){ }

            try
            {
                sellerName = jObject["ad"]!["ad"]!["user"]!["name"]!.ToString();
            }
            catch(Exception){ }
            
            try
            {
                adLocation = jObject["ad"]!["ad"]!["location"]!["pathName"]!.ToString();
            }
            catch(Exception){ }

            try
            {
                string priceValue = jObject["ad"]!["ad"]!["price"]!["regularPrice"]!["value"]!.ToString();
                string currency = jObject["ad"]!["ad"]!["price"]!["regularPrice"]!["currencyCode"]!.ToString();
                adPrice = Functions.convert_price(priceValue, currency);
            }
            catch(Exception){ }
            
            try
            {
                adImage = adDocument.DocumentNode.SelectSingleNode($"//img[@alt=\"{adTitle}\"]").GetAttributeValue("src", "");
            }
            catch(Exception)
            {
                adImage = errorImageUri;
            }
        
            
            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("dd.MM.yyyy"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("dd.MM.yyyy"), sellerType, "0", globalBlacklist);
            annoounCount++;
            
        }

        static async Task<String> GetPhoneNumber(long userId, string adId)
        {
            try
            {
                string link = GetPlatformPhoneNumber(adId)["phoneLink"];

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("accept", "*/*");
                client.DefaultRequestHeaders.Add("authorization", "Bearer 2cc892750521b56593925ea63c19e63e5a578d05");
                client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");

                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                string phoneNumber = jObject["data"]!["phones"]![0]!.ToString();

                if(String.IsNullOrEmpty(phoneNumber))
                {
                    return "null";
                }

                if(!phoneNumber.Contains('+'))
                {
                    phoneNumber = $"{GetPlatformPhoneNumber(adId)["phoneCode"]}{phoneNumber}".Replace(" ", String.Empty);
                }

                return phoneNumber;
            }
            catch(System.Net.Http.HttpRequestException)
            {
                string state = "ChangeToken";
                DB.UpdateState(userId, state);
                return "null";
            }
        }

        static Dictionary<string, string> GetPlatformPhoneNumber(string adId)
        {
            Dictionary<string, string> platformDict = new Dictionary<string, string>();
            string phoneLink = "";
            string phoneCode = "";

            switch(userPlatform)
            {
                case "olx.ro":
                    phoneLink = $"https://www.olx.ro/api/v1/offers/{adId}/limited-phones/";
                    phoneCode = "+4";
                    break;
                case "olx.pt":
                    phoneLink = $"https://www.olx.pt/api/v1/offers/{adId}/limited-phones/";
                    phoneCode = "+351";
                    break;
                case "olx.pl":
                    phoneLink = $"https://www.olx.pl/api/v1/offers/{adId}/limited-phones/";
                    phoneCode = "+48";
                    break;
                case "olx.bg":
                    phoneLink = $"https://www.olx.bg/api/v1/offers/{adId}/limited-phones/";
                    phoneCode = "+359";
                    break;
            }

            platformDict.Add("phoneLink", phoneLink);
            platformDict.Add("phoneCode", phoneCode);

            return platformDict;
        }

        static string GenerateLink(string link, int page)
        {
            string pageLink = "";

            if(link.Contains("https://m.olx"))
            {
                link = link.Replace("https://m.olx", "https://www.olx");
            }

            switch(userPlatform)
            {
                case "olx.ro":
                    if(link.Contains("https://www.olx.ro/"))
                    {
                        if(link[^1] == '/')
                        {
                            pageLink = link + "?page=" + page.ToString();
                        }
                        else
                        {
                            pageLink = link + "/?page=" + page.ToString();
                        }
                    }
                    else
                    {
                        pageLink = $"https://www.olx.ro/d/oferte/q-{link}/?page={page}";
                    }
                    break;
                case "olx.pt":
                    if(link.Contains("https://www.olx.pt/"))
                    {
                        if(link[^1] == '/')
                        {
                            pageLink = link + "?page=" + page.ToString();
                        }
                        else
                        {
                            pageLink = link + "/?page=" + page.ToString();
                        }
                    }
                    else
                    {
                        pageLink = $"https://www.olx.pt/d/ads/q-{link}/?page={page}";
                    }
                    break;
                case "olx.pl":
                    if(link.Contains("https://www.olx.pl/"))
                    {
                        if(link[^1] == '/')
                        {
                            pageLink = link + "?page=" + page.ToString();
                        }
                        else
                        {
                            pageLink = link + "/?page=" + page.ToString();
                        }
                    }
                    else
                    {
                        pageLink = $"https://www.olx.pl/d/oferty/q-{link}/?page={page}";
                    }
                    break;
                case "olx.bg":
                    if(link.Contains("https://www.olx.bg/"))
                    {
                        if(link[^1] == '/')
                        {
                            pageLink = link + "?page=" + page.ToString();
                        }
                        else
                        {
                            pageLink = link + "/?page=" + page.ToString();
                        }
                    }
                    else
                    {
                        pageLink = $"https://www.olx.bg/d/ads/q-{link}/?page={page}";
                    }
                    break;
            }
            return pageLink;
        }
    }
}