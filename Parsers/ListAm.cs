using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class ListAm
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static HtmlWeb web = new HtmlWeb();
        
        public static int StartParsing(long userId)
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
                    var advertisements = document.DocumentNode.SelectNodes("//div[@class=\"dl\"]//a");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string linkLanguage = advertisement.GetAttributeValue("href", "").Split('/')[1];
                                string subLink = advertisement.GetAttributeValue("href", "").Split(linkLanguage)[1];
                                string adLink = "https://www.list.am/" + linkLanguage + subLink;

                                adsPassed++;
                                DB.UpdateStatistic(userId, pagesPassed, adsPassed);

                                if(!DB.CheckAdvestisement(userId, adLink))
                                {
                                    HtmlDocument adDocument = web.Load(adLink);
                                    string adId = adLink.Split('/')[^1];
                                    string sellerPhoneNumber;
                                    string sellerLink = "Не указана";

                                    try
                                    {
                                        sellerLink = "https://www.list.am" + adDocument.DocumentNode.SelectSingleNode("//div[@id=\"uinfo\"]").SelectSingleNode(".//a[@class=\"n\"]").GetAttributeValue("href", "");
                                    }
                                    catch(Exception){  }
                                    
                                    if(sellerLink != "Не указана")
                                    {
                                        if(Functions.CheckIfSellerLinkNotExists(userId, sellerLink, globalBlacklist, localBlacklist)){ }else{continue;}
                                    }

                                    try
                                    {
                                        var ifAdContainsPhone = adDocument.DocumentNode.SelectSingleNode("//div[@class=\"phone\"]");
                                    }
                                    catch(Exception){ continue; }

                                    sellerPhoneNumber = GetPhoneNumber(adId).Result;

                                    if(sellerPhoneNumber != "null")
                                    {
                                        if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist))
                                        {
                                            ParsAdInfo(userId, adDocument, adLink, sellerLink, sellerPhoneNumber, userSellerAdCount, userSellerRegDate, userAdvRegDate, userSellerType, localBlacklist, globalBlacklist);
                                        }
                                    }
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
                        string state = "StopParser";
                        DB.UpdateState(userId, state);
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                        return 1;
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

        static void ParsAdInfo(long userId, HtmlDocument adDocument, string adLink, string sellerLink, string sellerPhoneNumber, string userSellerAdCount, string userSellerRegDate, string userAdvRegDate, string userSellerType, string localBlacklist, string globalBlacklist)
        {
            string adTitle = "Не указано";
            string adPrice = "Не указана";
            string adImage = errorImageUri;
            string sellerType = "Частное лицо";
            string sellerName = "Не указано";
            string adLocation = "Не указано";
            int sellerTotalAds = 1;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;

            try
            {
                var labels = adDocument.DocumentNode.SelectNodes("//span[@class=\"clabel\"]");
                foreach (HtmlNode label in labels)
                {
                    if(label.InnerText.Contains("Агентство") || label.InnerText.Contains("Организация"))
                    {
                        sellerType = "Бизнесс аккаунт";
                    }
                }
            }
            catch(Exception){  }

            if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }

            try
            {
                adRegDate = Convert.ToDateTime(adDocument.DocumentNode.SelectSingleNode("//span[@itemprop=\"datePosted\"]").GetAttributeValue("content", ""));
            }
            catch(Exception){  }

            if(Functions.check_adv_reg_data(userAdvRegDate, adRegDate)){ }else{ return; }

            try
            {
                sellerRegDate = Convert.ToDateTime(adDocument.DocumentNode.SelectSingleNode("//div[@id=\"uinfo\"]").SelectSingleNode(".//div[@class=\"since\"]").InnerText.Split(" ")[^1]);
            }
            catch(Exception){  }

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                HtmlDocument sellerDocument = web.Load(sellerLink);
                sellerTotalAds = 0;
                
                var sellerAdsCategories = sellerDocument.DocumentNode.SelectNodes("//div[@class=\"cl\"]//div[@class=\"s\"]//a");
                foreach (HtmlNode sellerAdsCategory in sellerAdsCategories)
                {
                    int categoryAds = Functions.leave_only_numbers(sellerAdsCategory.InnerText);
                    sellerTotalAds += categoryAds;
                }
            }
            catch(Exception){ sellerTotalAds = 1; }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = adDocument.DocumentNode.SelectSingleNode("//h1[@itemprop=\"name\"]").InnerText;
            }
            catch(Exception){  }

            try
            {
                string adAmount = adDocument.DocumentNode.SelectSingleNode("//span[@class=\"price\"]").GetAttributeValue("content", "");
                string adCurrency = adDocument.DocumentNode.SelectSingleNode("//meta[@itemprop=\"priceCurrency\"]").GetAttributeValue("content", "");
                adPrice = Functions.convert_price(adAmount, adCurrency);
            }
            catch(Exception){  }

            try
            {
                adLocation = adDocument.DocumentNode.SelectSingleNode("//div[@class=\"loc\"]").InnerText;
            }
            catch(Exception){  }

            try
            {
                adImage = "https:" + adDocument.DocumentNode.SelectSingleNode("//img[@itemprop=\"image\"]").GetAttributeValue("src", "").Replace(".webp", ".jpg");
            }
            catch(Exception){  }

            try
            {
                sellerName = adDocument.DocumentNode.SelectSingleNode("//div[@id=\"uinfo\"]").SelectSingleNode(".//a[@class=\"n\"]//div").InnerText;
            }
            catch(Exception){  }

            
            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, "0", globalBlacklist);
            annoounCount++;
            
        }

        static async Task<String> GetPhoneNumber(string adId)
        {
            try
            {
                string link = $"https://www.list.am/?w=12&&i={adId}";

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Add("cookie", "lang=1; _gid=GA1.2.833379606.1663434706; __stripe_mid=e5f0b208-37ae-4a5e-ad34-a0ff1f6091c7b1daaa; u=0005w3260044d04402223468cc3e7b5b99df002f7393a4abe27481d; __stripe_sid=3bbbb66b-b65d-42a8-928a-aaa86e9fd57896947c; _gat=1; _ga_KVLP4BC4K8=GS1.1.1663508066.3.1.1663508852.0.0.0; _ga=GA1.1.1841038172.1663434705");
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var htmlResponse = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlResponse);

                string phoneNumber = Functions.convert_phone(htmlDocument.DocumentNode.SelectSingleNode("//div[@class=\"phones\"]//a").InnerText);

                if(String.IsNullOrEmpty(phoneNumber))
                {
                    return "null";
                }


                if(!phoneNumber.Contains("+"))
                {
                    if(phoneNumber.Substring(0, 3) == "374")
                    {
                        phoneNumber = $"+{phoneNumber}";
                    }
                    else
                    {
                        phoneNumber = $"+374{phoneNumber}";
                    }
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

            if(link.Contains("https://www.list.am/"))
            {
                if(link[^1] == '/')
                {
                    pageLink = link + page.ToString();
                }
                else
                {
                    pageLink = link + "/" + page.ToString();
                }
            }
            else
            {
                pageLink = $"https://www.list.am/category?q={link}&pg={page}";
            }
            return pageLink;
        }
    }
}