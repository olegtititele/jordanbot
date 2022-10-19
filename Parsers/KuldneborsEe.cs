using PostgreSQL;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Parser
{
    public class KuldneborsEe
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static int pagesPassed = 1;
        private static int adsPassed = 0;
        

        public static int StartParsing(long userId)
        {
            annoounCount = 0;
            pagesPassed = 1;
            adsPassed = 0;

            var options = new FirefoxOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-setuid-sandbox");
            options.AddArgument($"--user-agent={userAgent}");
            options.AddArgument("--disable-gpu");
            options.AddArguments("--headless");
            IWebDriver driver = new FirefoxDriver(options);


            try
            {   
                userPlatform = DB.GetPlatform(userId);
                string userLink = DB.GetLink(userId);
                int userAnnounCount = DB.GetAnnounCount(userId);
                string userAdRegDate = DB.GetAdvRegData(userId);
                string userSellerType = DB.GetSellerType(userId);
                int page = DB.GetStartPage(userId);
                string localBlacklist = DB.GetLocalAndGlobalBl(userId)[0];
                string globalBlacklist = DB.GetLocalAndGlobalBl(userId)[1];


                while(true)
                {
                    if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                    {
                        driver.Navigate().GoToUrl(GenerateLink(userSellerType, userLink, page));
                        
                        var advertisements = driver.FindElements(By.XPath("//h4[@class=\"kb-object__heading \"]//a[@target=\"_self\"]"));
                        if(advertisements != null)
                        {
                            List<string> advertisementsLinks = new List<string>();
                            foreach(var advertisement in advertisements)
                            {
                                string adLink = advertisement.GetAttribute("href").Replace("&pob_browser_offset=10", "&pob_browser_offset=0");
                                if(!adLink.Contains("&pob_browser_offset"))
                                {
                                    adLink = adLink + "&pob_browser_offset=0";
                                }
                                advertisementsLinks.Add(adLink);
                            }

                            GetLinks(driver, advertisementsLinks, userAnnounCount, userId, localBlacklist, globalBlacklist, userAdRegDate);
                            page++;
                            pagesPassed++;
                        }
                        else
                        {
                            string state = "StopParser";
                            DB.UpdateState(userId, state);
                            DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                            try
                            {
                                driver.Close(); 
                            }
                            catch{}
                            
                            return 1;
                        }
                    }
                    else
                    {
                        string state = "StopParser";
                        DB.UpdateState(userId, state);
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                        try
                        {
                            driver.Close(); 
                        }
                        catch{}
                        return 1;
                    }
                }
            }
            catch(Exception)
            {
                string state = "StopParser";
                DB.UpdateState(userId, state);
                DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                try
                {
                    driver.Close(); 
                }
                catch{}
                return 1;
            }
        }


        private static void GetLinks(IWebDriver driver, List<string> advertisementsLinks, int userAnnounCount, long userId, string localBlacklist, string globalBlacklist, string userAdRegDate)
        {
            foreach(string adLink in advertisementsLinks)
            {
                if(!DB.CheckAdvestisement(userId, adLink))
                {
                    if(annoounCount < userAnnounCount && DB.GetState(userId)=="Parser")
                    {
                        Console.WriteLine(adLink);
                        adsPassed++;
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                        System.Threading.Thread.Sleep(1000);
                        getPageInfo(driver, adLink, userId, localBlacklist, globalBlacklist, userAdRegDate);
                    }
                    else
                    {
                        string state = "StopParser";
                        DB.UpdateState(userId, state);
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                    }
                }
                continue;
            }
        }


        static void getPageInfo(IWebDriver driver, string adLink, long userId, string localBlacklist, string globalBlacklist, string userAdRegDate)
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


            driver.Navigate().GoToUrl(adLink);

            try
            {
                var script = driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).GetAttribute("onclick").Split(';')[0] + ';';
                Console.WriteLine(script);
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript(script);
//                 var element = driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a"));
//                 IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                
// //                 var element1 = driver.FindElement(By.XPath("//div[@class=\"onetrust-pc-dark-filter ot-fade-in\"]"));
// //                 js.ExecuteScript("arguments[0].remove();", element1);
                
//                 js.ExecuteScript("arguments[0].click();", element);

                sellerPhoneNumber = driver.FindElement(By.XPath("//span[@id=\"contact-phones\"]")).Text.Trim().Replace(" ", "");
                Console.WriteLine(sellerPhoneNumber);

                if(!sellerPhoneNumber.Contains("+"))
                {
                    if(sellerPhoneNumber.Substring(0, 3) == "372")
                    {
                        sellerPhoneNumber = $"+{sellerPhoneNumber}";
                    }
                    else
                    {
                        sellerPhoneNumber = $"+372{sellerPhoneNumber}";
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ return; }


            try
            {
                adRegDate = Convert.ToDateTime(driver.FindElement(By.XPath("//div[@class=\"kb-object-view__meta-data\"]")).Text.Split(" ")[^1]);
            }
            catch{}

            if(Functions.check_adv_reg_data(userAdRegDate, adRegDate)){ }else{ return; }


            try
            {
                adTitle = driver.FindElement(By.XPath("//h1[@class=\"kb-object-view__heading\"]")).Text.Trim();
            }
            catch{}
            
            try
            {
                adPrice = Functions.convert_price(driver.FindElement(By.XPath("//span[@id=\"price\"]")).Text, "EUR");
            }
            catch{}

            try
            {
                adLocation = driver.FindElement(By.XPath("//div[@class=\"kb-object-view__owner\"]")).Text.Split("Näita kaarti")[0].Trim().Split("\n")[^1];

                if(adLocation.Contains("Tel.") || String.IsNullOrEmpty(adLocation))
                {
                    adLocation = "Не указано";
                }
            }
            catch{}

            try
            {
                adImage = driver.FindElement(By.XPath($"//img[@alt=\"{adTitle}\"]")).GetAttribute("src");
            }
            catch{}

            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
        }


        static string GenerateLink(string userSellerType, string link, int page)
        {
            string pageLink = "";

            if(link.Contains("https://www.kuldnebors.ee/"))
            {
                https://www.kuldnebors.ee/search/telefonid/search.mec?pob_evt=onpageindex&pob_action=search&pob_cat_id=10684&pob_page_index=1&pob_page_size=50&search_O_user_types=-R&pob_evt_param=5
                if(userSellerType == "Частное лицо")
                {
                    if(link.Contains("search_O_user_types"))
                    {
                        pageLink = $"{link.Replace("&search_O_deal_type=M", "&search_O_user_types=-R")}&pob_evt_param={page}&pob_evt=onpageindex&pob_page_index=1";
                    }
                    else
                    {
                        pageLink = $"{link}&search_O_user_types=-R&pob_evt_param={page}&pob_evt=onpageindex&pob_page_index=1";
                    }
                }
                else
                {
                    pageLink = $"{link}&pob_evt_param={page}";
                }                
            }
            else
            {
                if(userSellerType == "Частное лицо")
                {
                    pageLink = $"https://www.kuldnebors.ee/search/search.mec?search_evt=onsearch&pob_action=search&search_O_string={link}&search_O_user_types=-R&pob_evt_param={page}&pob_evt=onpageindex&pob_page_index=1";
                }
                else
                {
                     pageLink = $"https://www.kuldnebors.ee/search/search.mec?search_evt=onsearch&pob_action=search&search_O_string={link}&pob_evt_param={page}&pob_evt=onpageindex&pob_page_index=1";
                }
            }
            return pageLink;
        }
    }
}
