using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PostgreSQL;


namespace Parser
{
    public class OpensooqCom
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1";
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
                    var advertisements = document.DocumentNode.SelectNodes("//a[contains(@data-ghost, 'SERP-PostImage')]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string adLink = "";
                                switch(userPlatform)
                                {
                                    case "om.opensooq.com":
                                        adLink = "https://om.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "ye.opensooq.com":
                                        adLink = "https://ye.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "ae.opensooq.com":
                                        adLink = "https://ae.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "iq.opensooq.com":
                                        adLink = "https://iq.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "eg.opensooq.com":
                                        adLink = "https://eg.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "lb.opensooq.com":
                                        adLink = "https://lb.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "ly.opensooq.com":
                                        adLink = "https://ly.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "sa.opensooq.com":
                                        adLink = "https://sa.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                    case "sd.opensooq.com":
                                        adLink = "https://sd.opensooq.com"  + advertisement.GetAttributeValue("href", "").Replace("/ar/", "/en/");
                                        break;
                                }
                                
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
            JObject? jObject = null;
            

            web.UserAgent = userAgent;
            HtmlDocument adDocument = web.Load(adLink);

            try
            {
                var script = adDocument.DocumentNode.SelectSingleNode("//script[@id=\"__state\"]").InnerText;
                if(script.Contains("os.__STATE="))
                {
                    string json = script.Split("os.__STATE=")[1];
                    jObject = JObject.Parse(json);
                }
            }
            catch{ return; }

            try
            {
                string phoneNumberCode = jObject!["PostStore"]!["_post"]!["member"]!["phone_country_code"]!.ToString();
                string phoneNumber = jObject!["PostStore"]!["_post"]!["member"]!["phone_number"]!.ToString();

                if(String.IsNullOrEmpty(phoneNumber))
                {
                    return;
                }
                sellerPhoneNumber = "+" + phoneNumberCode + phoneNumber;
            }
            catch{ return; }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ return; }
            
            try
            {
                adRegDate = Convert.ToDateTime(jObject["PostStore"]!["_post"]!["mediasWith360"]![0]!["record_insert_date"]!.ToString());
            }
            catch{  }

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }

            try
            {
                bool isShop = Convert.ToBoolean(jObject["PostStore"]!["_post"]!["member"]!["isShop"]!.ToString());

                if(isShop)
                {
                    sellerType = "Бизнесс аккаунт";
                }
                else
                {
                    sellerType = "Частное лицо";
                }
            }
            catch{  }

            if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }

            try
            {
                sellerRating = decimal.Parse(jObject["PostStore"]!["_post"]!["member"]!["rating"]!["average_rating"]!.ToString());
            }
            catch{  }

            if(Functions.CheckSellerRating(userSellerRating, sellerRating)){ }else{ return; }

            try
            {
                sellerRegDate = Convert.ToDateTime(jObject["PostStore"]!["_post"]!["member"]!["record_insert_date"]!.ToString());
            }
            catch{  }

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                sellerTotalAds = Int32.Parse(jObject["PostStore"]!["_post"]!["owner_post_count"]!.ToString());
            }
            catch{  }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = jObject["PostStore"]!["_post"]!["title"]!.ToString();
            }
            catch{  }

            try
            {
                string adAmount = jObject["PostStore"]!["_post"]!["price_list"]!.ToString().Split("\"price_d\": ")[1].Split(',')[0];
                string adCurrency = jObject["PostStore"]!["_post"]!["price_list"]!.ToString().Split("\"price_code\": ")[1].Split('"')[1];
                adPrice = Functions.convert_price(adAmount, adCurrency);
            }
            catch{  }

            try
            {
                sellerName = jObject["PostStore"]!["_post"]!["member"]!["full_name"]!.ToString();
                sellerLink = "https://om.opensooq.com" + jObject["PostStore"]!["_post"]!["member"]!["memberLink"]!.ToString();
            }
            catch{  }

            try
            {
                var blocks = adDocument.DocumentNode.SelectSingleNode("//ul[@class=\"customParams smallFont\"]").SelectNodes("//li[@class=\"flex flexNoWrap alignItems\"]");
                string adLocationBlock = "";
                foreach(var block in blocks)
                {
                    if(block.InnerText.Contains("City"))
                    {
                        string city = block.InnerText.Split("City")[1];
                        adLocationBlock += city;
                    }

                    if(block.InnerText.Contains("Location"))
                    {
                        string location = block.InnerText.Split("Location")[1];
                        adLocationBlock += " - " + location;
                    }
                }
                adLocation = adLocationBlock;
            }
            catch{  }

            try
            {
                adImage = "https:" + adDocument.DocumentNode.SelectSingleNode($"//noscript//img[@alt=\"{adTitle.Replace("\"", "&quot;")}\"]").GetAttributeValue("src", "");
            }
            catch{  }


            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
            
        }


        static string GenerateLink(long userId, string link, int page)
        {
            string pageLink = "";
            if(link.Contains("https://"))
            {
                pageLink = link.TrimEnd('/') + "?page=" + page.ToString();
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