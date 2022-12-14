using PostgreSQL;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
namespace Parser
{
    public class KuldneborsEe
    {
        private static string userPlatform = "";
        private static int annoounCount = 0;
        private static string userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
        private static string errorImageUri = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
        private static string goodLink = "https://www.kuldnebors.ee/search/telefonid/mobiiltelefonide-lisaseadmed/iphone-13-pro-back-case/search.mec?pob_post_id=86525661&pob_action=show_post&pob_cat_id=10946&pob_browser_offset=30&pob_view_language_id=et&search_evt=onsearch&search_O_string=Apple&search_O_user_types=-R&pob_page_index=1";
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
                                string adLink = advertisement.GetAttribute("href");
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
                        adsPassed++;
                        DB.UpdateStatistic(userId, pagesPassed, adsPassed);
                        System.Threading.Thread.Sleep(300);
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
            string adTitle = "???? ??????????????";
            string adPrice = "???? ??????????????";
            string adImage = errorImageUri;
            string sellerType = "?????????????? ????????";
            string sellerName = "???? ??????????????";
            string sellerLink = "???? ??????????????";
            string adLocation = "???? ??????????????";
            string sellerPhoneNumber = "";
            int sellerTotalAds = 1;
            decimal sellerRating = 0.0M;
            DateTime adRegDate = DateTime.Today;
            DateTime sellerRegDate = DateTime.Today;
            string script;
            IWebElement phoneNumberBlock;

            driver.Navigate().GoToUrl(adLink);
            System.Threading.Thread.Sleep(2000);
            try
            {
                phoneNumberBlock = driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a"));
                script = phoneNumberBlock.GetAttribute("onclick");
            }
            catch
            {
                return;
            }

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
                adLocation = driver.FindElement(By.XPath("//div[@class=\"kb-object-view__owner\"]")).Text.Split("N??ita kaarti")[0].Trim().Split("\n")[^1];

                if(adLocation.Contains("Tel.") || String.IsNullOrEmpty(adLocation))
                {
                    adLocation = "???? ??????????????";
                }
            }
            catch{}

            try
            {
                adImage = driver.FindElement(By.XPath($"//img[@alt=\"{adTitle}\"]")).GetAttribute("src");
            }
            catch{}

            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript($"arguments[0].setAttribute(\"onclick\", \"{script}\");", phoneNumberBlock);
// //                 js.ExecuteScript("document.getElementById('contact-phones').click();");
//                 try
//                 {
                    
//                     js.ExecuteScript($"arguments[0].remove();", driver.FindElement(By.XPath("//div[@class=\"onetrust-pc-dark-filter ot-fade-in\"]")));
//                 }
//                 catch{}
//                 js.ExecuteScript("arguments[0].scrollIntoView();", driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")));
                  
//                 driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).Click();
                System.Threading.Thread.Sleep(1000);
// //                 driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).Click();
// //                 driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).Click();
// //                 driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).Click();
// //                 driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")).Click();
// //                 System.Threading.Thread.Sleep(10000);
//                 sellerPhoneNumber = driver.FindElement(By.XPath("//span[@id=\"contact-phones\"]")).Text.Trim();
                
//                 if(sellerPhoneNumber == "N??ita numbrit")
//                 {
//                     driver.Navigate().GoToUrl(goodLink);
//                     int attempts = 0;
//                     bool result = false;
// //                     while(attempts < 3) 
// //                     {
// //                         try 
// //                         {
// //                             js.ExecuteScript($"arguments[0].setAttribute(\"onclick\", \"{script}\");", driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")));
// //                             System.Threading.Thread.Sleep(500);
// //                             result = true;
// //                             break;
// //                         } 
// //                         catch(Exception e){ Console.WriteLine(e); }
// //                         attempts++;
// //                     }
                    
// //                     if(!result){ return; }
//                     js.ExecuteScript($"arguments[0].setAttribute(\"onclick\", \"{script}\");", driver.FindElement(By.XPath("//h1[@class=\"kb-object-view__heading\"]")));
//                     js.ExecuteScript($"arguments[0].setAttribute(\"onclick\", \"{script}\");", driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]/a")));
//                     System.Threading.Thread.Sleep(1000);
//                     Console.WriteLine(script);
//                     Console.WriteLine(driver.FindElement(By.XPath("//*[@id=\"contact-phones\"]")).GetAttribute("innerHTML"));
// //                     driver.FindElement(By.XPath("//h1[@class=\"kb-object-view__heading\"]")).Click();
                    
//                     js.ExecuteScript(script.Split(';')[0]);
//                     js.ExecuteScript(script.Split(';')[0]);
//                     js.ExecuteScript(script.Split(';')[0]);
// //                     js.ExecuteScript(script);
// //                     js.ExecuteScript(script);
//                     System.Threading.Thread.Sleep(1000);
//                 }
//                 removeChild(elem)
                sellerPhoneNumber = driver.FindElement(By.XPath("//span[@id=\"contact-phones\"]")).Text.Trim().Replace(" ", "");

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
                Console.WriteLine(adLink);
                Console.WriteLine(sellerPhoneNumber);
            }
            catch(Exception e){ Console.WriteLine(e); return; }

            if(Functions.check_blacklist_ads(userId, sellerPhoneNumber, globalBlacklist, localBlacklist)){ }else{ Console.WriteLine(sellerPhoneNumber); return; }

            Functions.InsertNewAd(userId, userPlatform, adTitle, adPrice, adRegDate.ToString("d"), adLink, adLocation, adImage, sellerName, sellerLink, sellerPhoneNumber, sellerTotalAds.ToString(), sellerRegDate.ToString("d"), sellerType, sellerRating.ToString(), globalBlacklist);
            annoounCount++;
        }


        static string GenerateLink(string userSellerType, string link, int page)
        {
            string pageLink = "";

            if(link.Contains("https://www.kuldnebors.ee/"))
            {
                if(userSellerType == "?????????????? ????????")
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
                if(userSellerType == "?????????????? ????????")
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
