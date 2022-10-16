using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Proxies;

using PostgreSQL;

namespace Parser
{
    public class BazarSk
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
                string userSellerType = DB.GetSellerType(userId);
                int page = DB.GetStartPage(userId);
                string localBlacklist = DB.GetLocalAndGlobalBl(userId)[0];
                string globalBlacklist = DB.GetLocalAndGlobalBl(userId)[1];


                while(true)
                {
                    web.UserAgent = userAgent;
                    HtmlDocument document = web.Load(GenerateLink(userLink, page), ParserProxy.myProxyIP, ParserProxy.myPort, ParserProxy.login, ParserProxy.password);
                    var advertisements = document.DocumentNode.SelectNodes("//div[@class=\"hmr-card\"]//a");
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
                                    ParsAdInfo(userId, adLink, userSellerAdCount, userSellerRegDate, userAdRegDate, userSellerType, localBlacklist, globalBlacklist);
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


        static void ParsAdInfo(long userId, string adLink, string userSellerAdCount, string userSellerRegDate, string userAdRegDate, string userSellerType, string localBlacklist, string globalBlacklist)
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
            

            web.UserAgent = userAgent;
            HtmlDocument adDocument = web.Load(adLink);



            try
            {
                var HavePhone = adDocument.DocumentNode.SelectSingleNode("//div[@class=\"phone-numbers phone-numbers--contact-box\"]").InnerHtml;
                var scripts = adDocument.DocumentNode.SelectNodes("//script[@type=\"text/javascript\"]");
                string json = "";
                foreach (var script in scripts)
                {
                    if(script.InnerText.Contains("WWW-Authenticate"))
                    {
                        json = script.InnerText.Split("window.auth = ")[1].Split(";")[0];
                        break;
                    }
                }
                JObject jObject = JObject.Parse(json);
                string token = jObject["token"]!.ToString();
                string adId = adDocument.DocumentNode.SelectSingleNode("//body").GetAttributeValue("data-id", "");
                string phoneHash = adDocument.DocumentNode.SelectSingleNode("//body").GetAttributeValue("data-phone-hash", "");
                 
                sellerPhoneNumber = GetPhoneNumber(adId, phoneHash, token).Result;
                if(sellerPhoneNumber=="null")
                {
                    return;
                }
                
            }
            catch{ return; }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ return; }

            try
            {
                var scripts = adDocument.DocumentNode.SelectNodes("//script[@type=\"application/ld+json\"]");
                string json = "";
                foreach (var script in scripts)
                {
                    if(script.InnerText.Contains("itemCondition"))
                    {
                        json = script.InnerText;
                        break;
                    }
                }
                JObject jObject = JObject.Parse(json);
                sellerName = jObject["offers"]!["seller"]!["name"]!.ToString();
                var TypeAcc = jObject["offers"]!["seller"]!["@type"]!.ToString();
                if(TypeAcc == "Organization")
                {
                    sellerType = "Бизнесс аккаунт";
                }
                else
                {
                    sellerType = "Частное лицо";
                }
            }
            catch{ }

            if(Functions.check_type_acc(userSellerType, sellerType)){ }else{ return; }

            try
            {
                adRegDate = Convert.ToDateTime(adDocument.DocumentNode.SelectSingleNode("//div[@class=\"block-title__info\"]").InnerText.Trim().Split("(")[0].Split("z dňa")[1]);
            }
            catch{ }

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }

            try
            {
                sellerRegDate = ConvertSellerDate(adDocument.DocumentNode.SelectSingleNode("//span[@class=\"seller-miniature__member-since\"]").InnerText);
            }
            catch{ }

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                sellerTotalAds = Functions.leave_only_numbers(adDocument.DocumentNode.SelectSingleNode("//a[@class=\"block-seller-info__all-items link-blue\"]").InnerText);
            }
            catch{ }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = adDocument.DocumentNode.SelectSingleNode("//h1[@class=\"block-title__heading\"]").InnerText.Trim();
            }
            catch{ }

            try
            {
                var AdvPriceBlock = adDocument.DocumentNode.SelectSingleNode("//div[@class=\"block-title__prices\"]").InnerText.Trim().Split("€")[0];
                adPrice = Functions.convert_price(AdvPriceBlock, "EUR");
            }
            catch{ }

            try
            {
                adImage = adDocument.DocumentNode.SelectSingleNode("//img[@class=\"block-gallery__preview-image block-gallery__preview-image--visible img-fluid\"]").GetAttributeValue("src", "");
            }
            catch{ }

            try
            {
                adLocation = adDocument.DocumentNode.SelectSingleNode("//address[@class=\"location-address\"]").InnerText.Trim();
            }
            catch{ }

            try
            {
                sellerLink = adDocument.DocumentNode.SelectSingleNode("//a[@class=\"seller-miniature__name seller-miniature__name--main \"]").GetAttributeValue("href", "");
            }
            catch{ }

            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
        }

        static async Task<string> GetPhoneNumber(string AdId, string PhoneHash, string token)
        {
            string responseLink = "https://mobily.bazar.sk/api/get-phone-number/?data%5Bid%5D=" + AdId + "&data%5BphoneHash%5D=" + PhoneHash;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("www-authenticate", token);
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            HttpResponseMessage response = await client.GetAsync(responseLink);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            if(json.Contains("{\"status\":401}"))
            {
                return "null";
            }
            string phoneBlock = Functions.convert_phone(json);
            string sellerPhoneNumber;
            if(!phoneBlock.Contains("+"))
            {
                if(phoneBlock.Substring(0, 3) == "421")
                {
                    sellerPhoneNumber = $"+{phoneBlock}";
                }
                else
                {
                    sellerPhoneNumber = $"+374{phoneBlock}";
                }
            }
            else
            {
                sellerPhoneNumber = phoneBlock;
            }

            return sellerPhoneNumber;
        }
        static string GenerateLink(string link, int page)
        {
            string pageLink;
            if(link.Contains("bazar.sk"))
            {
                if(link[^1]=='/')
                {
                    pageLink = link + "?p[page]=" + page.ToString();
                }
                else
                {
                    pageLink = link + "/?p[page]=" + page.ToString();
                }
            }
            else
            {
                pageLink = "https://www.bazar.sk/vyhladavanie/?p[keyword]=" + link + "&p[location]=&p[page]=" + page.ToString();
            }
            return pageLink;
        }

        static DateTime ConvertSellerDate(string data_block)
        {
            int day = 1;
            int month;
            string YearBlock = "";
            for (int i = 0; i < data_block.Length; i++)
            {
                if (Char.IsDigit(data_block[i]))
                    YearBlock += data_block[i];
                else 
                    continue;
            }
            int year = Int32.Parse(YearBlock);

            if(data_block.Contains("január"))
            {
                month = 1;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("februára"))
            {
                month = 2;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("marca"))
            {
                month = 3;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("apríla"))
            {
                month = 4;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("mája"))
            {
                month = 5;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("júna"))
            {
                month = 6;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("júla"))
            {
                month = 7;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("augusta"))
            {
                month = 8;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("septembra"))
            {
                month = 9;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("októbra"))
            {
                month = 10;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("novembra"))
            {
                month = 11;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("decembra"))
            {
                month = 12;
                DateTime reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else
            {
                DateTime reg_data_full = DateTime.Today;
                return reg_data_full;
            }
        }
    }
}