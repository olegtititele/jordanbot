using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class ArabOLX
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static HtmlWeb web = new HtmlWeb();
        
        public static int StartParsing(long user_id)
        {
            annoounCount = 0;
            int pagesPassed = 1;
            int adsPassed = 0;

            try
            {    
                List<string> parameters = DB.GetAllParameters(user_id);
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
                    
                    var advertisements = document.DocumentNode.SelectNodes("//li[@aria-label=\"Listing\"]//a");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(user_id)=="Parser")
                            {
                                string adLink = "";
                                switch(userPlatform)
                                {
                                    case "olx.qa":
                                        adLink = "https://www.olx.qa" + advertisement.GetAttributeValue("href", "");
                                        break;
                                    case "olx.com.om":
                                        adLink = "https://www.olx.com.om" + advertisement.GetAttributeValue("href", "");
                                        break;
                                }
                                adsPassed++;
                                DB.UpdateStatistic(user_id, pagesPassed, adsPassed);

                                if(!DB.CheckAdvestisement(user_id, adLink))
                                {
                                    string adId;
                                    string sellerPhoneNumber;
                                    try
                                    {
                                        adId = adLink.Split("ID")[1].Split('.')[0];
                                        sellerPhoneNumber = GetPhoneNumber(adId).Result;
                                    }
                                    catch(Exception){ continue; }
                                    
                                    if(sellerPhoneNumber != "null")
                                    {
                                        if(Functions.check_blacklist_ads(user_id, sellerPhoneNumber, globalBlacklist, localBlacklist))
                                        {
                                            ParsAdInfo(user_id, adLink, sellerPhoneNumber, userSellerAdCount, userSellerRegDate, userAdvRegDate, userSellerType, localBlacklist, globalBlacklist);
                                        }
                                    }
                                }
                                continue;
                            }
                            else
                            {
                                string state = "StopParser";
                                DB.UpdateState(user_id, state);
                                DB.UpdateStatistic(user_id, pagesPassed, adsPassed);
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        string state = "StopParser";
                        DB.UpdateState(user_id, state);
                        DB.UpdateStatistic(user_id, pagesPassed, adsPassed);
                        return 1;
                    }
                    pagesPassed++;
                    page++;
                }
            }
            catch(Exception)
            {
                string state = "StopParser";
                DB.UpdateState(user_id, state);
                DB.UpdateStatistic(user_id, pagesPassed, adsPassed);
                return 1;
            }

        }

        static void ParsAdInfo(long user_id, string adLink, string sellerPhoneNumber, string userSellerAdCount, string userSellerRegDate, string userAdvRegDate, string userSellerType, string localBlacklist, string globalBlacklist)
        {
            string adPrice = "";
            string adTitle = "";
            string adImage = "";
            string sellerType = "";
            string sellerLink = "";
            string sellerName = "";
            string adLocation = "";
            int sellerTotalAds = 1;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;

            web.UserAgent = userAgent;
            HtmlDocument document = web.Load(adLink);

            var scripts = document.DocumentNode.SelectNodes("//script");

            foreach (HtmlNode script in scripts)
            {
                if(script.InnerHtml.Contains("window['dataLayer'] = window['dataLayer'] || [];window['dataLayer'].push("))
                {
                    string json = script.InnerHtml.Split("window['dataLayer'] = window['dataLayer'] || [];window['dataLayer'].push(")[1];
                    char[] MyChar = {';',')'};
                    json = json.TrimEnd(MyChar);
                    JObject jObject = JObject.Parse(json);

                    try
                    {
                        switch(userPlatform)
                        {
                            case "olx.qa":
                                adPrice = Functions.convert_price(jObject["price"]!.ToString(), "QAR");
                                break;
                            case "olx.com.om":
                                adPrice = Functions.convert_price(jObject["price"]!.ToString(), "OMR");
                                break;
                        }
                    }
                    catch(Exception)
                    {
                        adPrice = "Не указана";
                    }

                    // string adId = jObject["ad_id"]!.ToString();
                    adTitle = jObject["ad_title"]!.ToString();
                    sellerType = jObject["seller_type"]!.ToString();

                    if(sellerType == "individual")
                    {
                        sellerType = "Частное лицо";
                    }
                    else
                    {
                        sellerType = "Бизнесс аккаунт";
                    }

                    if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }

                    break;
                }
            }
            foreach (HtmlNode script in scripts)
            {
                if(script.InnerHtml.Contains("priceValidUntil"))
                {
                    string json = script.InnerText;
                    JObject jObject = JObject.Parse(json);
                    adRegDate = Convert.ToDateTime(jObject["offers"]![0]!["priceValidUntil"]!.ToString()).AddDays(-30);

                    if(Functions.check_adv_reg_data(userAdvRegDate, adRegDate)){ }else{ return; }
                    break;
                }
            }
            foreach (HtmlNode script in scripts)
            {
                if(script.InnerHtml.Contains("window.state = "))
                {
                    string json = script.InnerHtml.Split("window.state = ")[1].Split("\n")[0];
                    char[] MyChar = {';',')'};
                    json = json.TrimEnd(MyChar);
                    JObject jObject = JObject.Parse(json);

                    var sellerProfile = jObject["sellerProfile"]!;
                    string sellerId = sellerProfile["data"]!["externalID"]!.ToString();
                    switch(userPlatform)
                    {
                        case "olx.qa":
                            sellerLink = $"https://www.olx.qa/profile/{sellerId}";
                            break;
                        case "olx.com.om":
                            sellerLink = $"https://www.olx.com.om/profile/{sellerId}";
                            break;
                    }
                    
                    sellerName = sellerProfile["data"]!["name"]!.ToString();
                    sellerRegDate = Convert.ToDateTime(sellerProfile["data"]!["createdAt"]!.ToString());

                    if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }
                    break;
                }
            }

            try
            {
                HtmlDocument sellerDocument = web.Load(sellerLink);
                
                sellerTotalAds = Functions.leave_only_numbers(sellerDocument.DocumentNode.SelectSingleNode("//span[@class=\"_34a7409b _6a44af43 _2e82a662\"]").InnerText);
            }
            catch(Exception)
            {
                sellerTotalAds = 1;
            }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adLocation = document.DocumentNode.SelectSingleNode("//span[@aria-label=\"Location\"]").InnerText;
            }
            catch(Exception)
            {
                adLocation = "Не указано";
            }

            try
            {
                adImage = document.DocumentNode.SelectSingleNode("//img[@role=\"presentation\"]").GetAttributeValue("src", "").Replace("240x180.jpeg", "400x300.jpeg");
            }
            catch(Exception)
            {
                adImage = errorImageUri;
            }
            
            Functions.InsertNewAd(user_id, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, "0", globalBlacklist);
            annoounCount++;
            
        }

        static async Task<String> GetPhoneNumber(string adId)
        {
            try
            {
                string link = "";
                switch(userPlatform)
                {
                    case "olx.qa":
                        link = $"https://www.olx.qa/api/listing/{adId}/contactInfo/";
                        break;
                    case "olx.com.om":
                        link = $"https://www.olx.com.om/api/listing/{adId}/contactInfo/";
                        break;
                }

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("accept", "application/json");
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                string phoneNumber = jObject["mobile"]!.ToString();

                if(String.IsNullOrEmpty(phoneNumber))
                {
                    return "null";
                }

                if(!phoneNumber.Contains('+'))
                {
                    phoneNumber = $"+{phoneNumber}";
                }

                return phoneNumber;
            }
            catch(Exception)
            {
                return "null";
            }
        }

        static string GenerateLink(string link, int page)
        {
            string pageLink = "";

            switch(userPlatform)
            {
                case "olx.qa":
                    if(link.Contains("https://www.olx.qa/"))
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
                        pageLink = $"https://www.olx.qa/ads/q-{link}/?page={page}";
                    }
                    return pageLink;
                case "olx.com.om":
                    if(link.Contains("https://www.olx.com.om/"))
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
                        pageLink = $"https://www.olx.com.om/ads/q-{link}/?page={page}";
                    }
                    return pageLink;
            }
            return pageLink;
        }
    }
}