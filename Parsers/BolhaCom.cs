using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class BolhaCom
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
                    HtmlDocument document = web.Load(GenerateLink(userLink, page));
                    var advertisements = document.DocumentNode.SelectNodes("//li[contains(@class, 'EntityList-item EntityList-item--Regular')]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                            {
                                string adLink = "https://www.bolha.com" + advertisement.GetAttributeValue("data-href", "");

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
            HtmlDocument sellerDocument; 
            

            web.UserAgent = userAgent;
            HtmlDocument adDocument = web.Load(adLink);

            try
            {
                sellerPhoneNumber = Functions.convert_phone(adDocument.DocumentNode.SelectSingleNode("//a[@class=\"ClassifiedDetailOwnerDetails-contactEntryLink link-tel link-tel--gamma link-tel--faux\"]").GetAttributeValue("data-display", ""));
                if(String.IsNullOrWhiteSpace(sellerPhoneNumber)){ return; }
            }
            catch{ return; }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ return; }

            try
            {
                adRegDate = Convert.ToDateTime(adDocument.DocumentNode.SelectSingleNode("//dd[@class=\"ClassifiedDetailSystemDetails-listData\"]").InnerText.Split(". ob")[0].Trim());
            }
            catch{ }

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }

            try
            {
                sellerName = adDocument.DocumentNode.SelectSingleNode("//a[@class=\"link-standard \"]").InnerText.Trim();
            }
            catch{ }

            try
            {
                sellerLink = adDocument.DocumentNode.SelectSingleNode("//a[@class=\"link-standard \"]").GetAttributeValue("href", "");
                sellerDocument = web.Load(sellerLink);
            }
            catch{ return; }

            try
            {
                string sellerInfo = sellerDocument.DocumentNode.SelectSingleNode("//p[@class=\"UserProfileDetails-subtitle\"]").InnerText.Trim();

                if(sellerInfo.Contains("Trgovina"))
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
                string sellerInfo = sellerDocument.DocumentNode.SelectSingleNode("//p[@class=\"UserProfileDetails-subtitle\"]").InnerText.Trim();

                string sellerRegDateBlock = sellerInfo.Split(" ")[^1].TrimEnd('.');
                sellerRegDate = Convert.ToDateTime(sellerRegDateBlock);
            }
            catch{ }

            if(Functions.check_seller_reg_data(userSellerRegDate, sellerRegDate)){ }else{ return; }

            try
            {
                sellerTotalAds = Int32.Parse(sellerDocument.DocumentNode.SelectSingleNode("//strong[@class=\"entities-count\"]").InnerText.Trim());
            }
            catch{ }

            if(Functions.check_seller_adv_count(userSellerAdCount, sellerTotalAds)){ }else{ return; }

            try
            {
                adTitle = adDocument.DocumentNode.SelectSingleNode("//h1[@class=\"ClassifiedDetailSummary-title\"]").InnerText.Trim();
            }
            catch{ }

            try
            {
                adPrice = Functions.convert_price(adDocument.DocumentNode.SelectSingleNode("//dd[@class=\"ClassifiedDetailSummary-priceDomestic\"]").InnerText, "EUR");
            }
            catch{ }

            try
            {
                adImage = adDocument.DocumentNode.SelectSingleNode("//img[contains(@class, 'ClassifiedDetailGallery-slideImage')]").GetAttributeValue("src", "");
            }
            catch{ }

            try
            {
                adLocation = adDocument.DocumentNode.SelectNodes("//span[@class=\"ClassifiedDetailBasicDetails-textWrapContainer\"]")[3].InnerText.Trim();
            }
            catch{ }
            
            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
        }

        static string GenerateLink(string link, int page)
        {
            string pageLink;
            if(link.Contains("https://www.bolha.com/"))
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
                
                pageLink = "https://www.bolha.com/?ctl=search_ads&keywords=" + link +"&page=" + page.ToString();
            }
            return pageLink;
        }
    }
}