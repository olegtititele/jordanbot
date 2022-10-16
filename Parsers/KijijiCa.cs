using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class KijijiCa
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static char[] MyChar = {';',')', '[', ']'};
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36";
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
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"title \"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string adLink = "https://www.kijiji.ca" + advertisement.GetAttributeValue("href", "");
                                Console.WriteLine(adLink);

                                adsPassed++;
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);

                                ParseCycle(userId, adLink, userSellerAdCount, userSellerRegDate, userAdRegDate, userSellerType, userSellerRating, localBlacklist, globalBlacklist);
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

        static void ParseCycle(long userId, string adLink, string userSellerAdCount, string userSellerRegDate, string userAdRegDate, string userSellerType, decimal userSellerRating, string localBlacklist, string globalBlacklist)
        {
            if(!DB.CheckAdvestisement(userId, adLink))
            {
                HtmlDocument adDocument = web.Load(adLink);
                string sellerPhoneNumber;
                string phoneToken = "";
                string adJson = "";

                try
                {
                    var scripts = adDocument.DocumentNode.SelectNodes("//script");

                    foreach (HtmlNode script in scripts)
                    {
                        if(script.InnerHtml.Contains("window.__data="))
                        {
                            adJson = script.InnerText.Split("window.__data=")[1];
                            adJson = adJson.Trim(MyChar);
                            JObject jObject = JObject.Parse(adJson);
                            phoneToken = jObject["profile"]!["phoneToken"]!.ToString().Replace("/", "%2F").Replace(":", "%3A");
                            break;
                        }
                    }
                }
                catch{ return; }

                sellerPhoneNumber = GetPhoneNumber(phoneToken).Result;

                if(sellerPhoneNumber != "null")
                {
                    if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist))
                    {
                        ParsAdInfo(userId, adDocument, adLink, sellerPhoneNumber, adJson, userSellerAdCount, userSellerRegDate, userAdRegDate, userSellerType, userSellerRating, localBlacklist, globalBlacklist);
                    }
                    return;
                }
            }
        }

        static void ParsAdInfo(long userId, HtmlDocument adDocument, string adLink, string sellerPhoneNumber, string adJson, string userSellerAdCount, string userSellerRegDate, string userAdRegDate, string userSellerType, decimal userSellerRating, string localBlacklist, string globalBlacklist)
        {
            string adTitle = "Не указано";
            string adPrice = "Не указана";
            string adImage = errorImageUri;
            string sellerType = "Частное лицо";
            string sellerName = "Не указано";
            string sellerLink = "Не указана";
            string adLocation = "Не указано";
            int sellerTotalAds = 1;
            decimal sellerRating = 0.0M;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;
            


            try
            {
                var sellerTypeBlock = adDocument.DocumentNode.SelectNodes($"//div[@class=\"line-2791721720\"]")[0].InnerText;

                if(sellerTypeBlock.Contains("Owner") || sellerTypeBlock.Contains("Private seller"))
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
                var scripts = adDocument.DocumentNode.SelectNodes("//script");

                foreach (HtmlNode script in scripts)
                {
                    if(script.InnerHtml.Contains("var dataLayer = ["))
                    {
                        string json = script.InnerText.Split("var dataLayer = ")[1].Split("\n")[0];
                        json = json.Trim(MyChar);
                        JObject jObject = JObject.Parse(json);
                        sellerRating = decimal.Parse(jObject["u"]!["ur"]!.ToString());
                        break;
                    }
                }
            }
            catch{}

            if(Functions.CheckSellerRating(userSellerRating, sellerRating)){ }else{ return; }
            
            try
            {
                adRegDate = Convert.ToDateTime(adDocument.DocumentNode.SelectSingleNode("//div[@itemprop=\"datePosted\"]//time").GetAttributeValue("datetime", ""));
            }
            catch{}

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }

            try
            {
                JObject jObject = JObject.Parse(adJson);
                sellerRegDate = Convert.ToDateTime(jObject["profile"]!["memberSince"]!.ToString());
            }
            catch{}

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                sellerTotalAds = Int32.Parse(adDocument.DocumentNode.SelectNodes($"//div[@class=\"line-2791721720\"]")[1].SelectSingleNode(".//span").InnerText);
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
                adImage = adDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'container-4202182046')]").SelectSingleNode(".//img").GetAttributeValue("src", "");
            }
            catch{}

            try
            {
                adLocation = adDocument.DocumentNode.SelectSingleNode("//span[@itemprop=\"address\"]").InnerText;
            }
            catch{}

            try
            {
                string adAmount = adDocument.DocumentNode.SelectSingleNode("//span[@itemprop=\"price\"]").InnerText;
                string adCurrency = adDocument.DocumentNode.SelectSingleNode("//span[@itemprop=\"priceCurrency\"]").GetAttributeValue("content", "");
                adPrice = Functions.convert_price(adAmount, adCurrency);
            }
            catch{}

            try
            {
                sellerLink = "https://www.kijiji.ca" + adDocument.DocumentNode.SelectSingleNode($"//a[@class=\"link-2686609741\"]").GetAttributeValue("href", "");
            }
            catch{}

            try
            {
                sellerName = adDocument.DocumentNode.SelectSingleNode($"//a[@class=\"link-2686609741\"]").InnerText;
            }
            catch{}

            
            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
            
        }

        static async Task<String> GetPhoneNumber(string phoneToken)
        {
            try
            {
                string link = "https://www.kijiji.ca/j-vac-phone-get.json?token=" + phoneToken;

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var htmlResponse = await response.Content.ReadAsStringAsync();

                JObject jObject = JObject.Parse(htmlResponse);
                string phoneNumber = Functions.convert_phone(jObject["phone"]!.ToString());


                if(String.IsNullOrEmpty(phoneNumber))
                {
                    return "null";
                }


                if(!phoneNumber.Contains("+"))
                {
                    phoneNumber = $"+1{phoneNumber}";
                }

                return phoneNumber;
            }
            catch(Exception)
            {
                return "null";
            }
        }

        static string GenerateLink(long userId, string link, int page)
        {
            link = link.TrimEnd('/');
            string pageLink = "";

            if(link.Contains("https://www.kijiji.ca/"))
            {
                string categoryId = link.Split('/')[^1];

                pageLink = link.Replace(categoryId, $"page-{page}") + $"/{categoryId}";
            }
            else
            {
                string state = "StopParser";
                DB.UpdateState(userId, state);
            }
            return pageLink;
        }
    }
}