using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class TuttiCh
    {
        private static int AnnCnd;
        private static string AdvTitle = "Не указано";
        private static string AdvLink = "";
        private static string AdvPrice = "Не указана";
        private static string AdvImage = "";
        private static string SellerName = "Не указано";
        private static string SellerLink = "Не указана";
        private static int SellerTotalAds = 1;
        private static DateTime AdvReg = DateTime.Today;
        private static string SellerReg = "Не указана";
        private static string AdvBusiness = "Частное лицо";
		private static string AdvLocation = "Не указано";
        private static string PhoneNumber = "Не указана";
        private static long user_id;
        private static string platform = "tutti.ch";
        private static int page;
        private static string link = "";
        private static int announ_count;
        private static string seller_adv_count="";
        private static string adv_reg_data = "";
		private static string seller_reg_data = "";
        private static string business = "";
        private static string local_blacklist = "";
        private static string global_blacklist = "";
        private static int PagesPassed = 1;
        private static int AdsPassed;
        private static HtmlWeb web = new HtmlWeb();


        public static int start_parsing(long usid)
        {
            user_id = usid;
            AnnCnd = 0;
            var parameters = DB.GetAllParameters(user_id);
            link = parameters[1];
            announ_count = Int32.Parse(parameters[2]);
            seller_adv_count = parameters[3];
            seller_reg_data = parameters[4];
            adv_reg_data = parameters[5];
            business = parameters[6];
            page = Int32.Parse(parameters[7]);
            local_blacklist = parameters[8];
            global_blacklist = parameters[9];
            PagesPassed = 1;
            AdsPassed = 0;
            

            while(true)
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument document = web.Load(generate_link(link, page));
                var advertisements = document.DocumentNode.SelectNodes("//div[@data-automation=\"ad\"]");
                if(advertisements != null)
                {
                    foreach (HtmlNode advertisement in advertisements)
                    {
                        if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                        {
                            AdvLink = "https://www.tutti.ch" + advertisement.SelectSingleNode(".//a[@data-automation=\"ad-subject\"]").GetAttributeValue("href", "");
                            try
                            {
                                var TypeAcc = advertisement.SelectSingleNode(".//div[@class=\"lazyload-wrapper \"]").InnerHtml;
                                AdvBusiness = "Бизнесс аккаунт";
                            }
                            catch(Exception)
                            {
                                AdvBusiness = "Частное лицо";
                            }

                            AdsPassed++;
                            DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                            if(DB.CheckAdvestisement(user_id, AdvLink))
                            {
                                continue;   
                            }
                            else
                            {
                                if(Functions.check_type_acc(business, AdvBusiness)){ pars_adv_info(); }else{ continue; }
                            }

                        }
                        else
                        {
                            string state = "StopParser";
                            DB.UpdateState(user_id, state);
                            DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                            return 1;
                        }
                    }    
                }
                else
                {
                    string state = "StopParser";
                    DB.UpdateState(user_id, state);
                    DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                    return 1;
                }
                PagesPassed++;
                page++;
            }
        }


        // Сгенерировать ссылку
        static string generate_link(string link, int page)
        {
            string page_link;
            if(link.Contains("https://www.tutti.ch/"))
            {
                if(link[^1]=='/')
                {
                    page_link = link.Substring(0, link.Length - 1) + "?o=" + page.ToString();
                    return page_link;
                }
                else
                {
                    page_link = link + "?o=" + page.ToString();
                    return page_link;
                }
            }
            else
            {
                page_link = "https://www.tutti.ch/de/li/ganze-schweiz?o=" + page.ToString() +"&q=" + link;
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);

            // Номер телефона продавца
            try
            {
                var scripts = adv_html.DocumentNode.SelectNodes("//script");
                foreach (HtmlNode script in scripts)
                {
                    if(script.InnerText.Contains("window.__REACT_QUERY_STATE__="))
                    {
                        string PhoneHash = script.InnerText.Split("\"phone_hash\":\"")[1].Split("\"")[0];
                        string AdvId = script.InnerText.Split("\"id\":\"")[1].Split("\"")[0];
                        // Console.WriteLine(PhoneHash, AdvId);
                        get_phone_number(PhoneHash, AdvId);
                    }
                }
            }
            catch (Exception){ return; }

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ Console.WriteLine(PhoneNumber); }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"_2MODw\"]").InnerText.Trim();
            }
            catch (Exception){ }
            Console.WriteLine(AdvTitle);
            
            // Цена объявления
            try
            {
                var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//h2[@class=\"dX6Ix\"]").InnerText.Trim();
                AdvPrice = Functions.convert_price(AdvPriceBlock.Substring(0, AdvPriceBlock.Length - 1), "CHF");
                
            }
            catch (Exception){ }

            
            // Ссылка на изображение
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode($"//div[@aria-label=\"{AdvTitle}\"]//img").GetAttributeValue("src", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }


            // Дата регистрации объявления
            try
            {
                AdvReg = convert_data(adv_html.DocumentNode.SelectSingleNode("//div[@class=\"_9mKtt pRm6L\"]//span").InnerText.Trim());
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//span[@class=\"ib-UH\"]").InnerText.Trim();
            }
            catch (Exception){ }

            // Имя продавца
            try
            {
                SellerName = adv_html.DocumentNode.SelectSingleNode("//h4[@class=\"qWtC7\"]").InnerText.Trim();
            }
            catch (Exception){ }

            // Cсылка на продавца
            try
            {
                SellerLink =  "https://www.tutti.ch" + adv_html.DocumentNode.SelectSingleNode("//a[@class=\"jta4a\"]").GetAttributeValue("href", "");
                // web.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML like Gecko) Chrome/31.0.1650.63 Safari/537.36";
                // Console.WriteLine(seller_link);
                // HtmlDocument seller_html = web.Load(seller_link);
                // Console.WriteLine(seller_html.DocumentNode.InnerHtml);
            }
            catch (Exception){ }

            // Добавить объявление в бд
            try
            {
                Functions.InsertNewAd(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg, AdvBusiness, "0", global_blacklist);
                AnnCnd++;
            }
            catch (Exception){ }
        }

        // Форматировать дату регистрации продавца
        static DateTime convert_data(string data_block)
        {
            DateTime reg_data_full;
            if(data_block.Contains("Heute"))
            {
                reg_data_full = DateTime.Today;
                return reg_data_full;
            }
            else if(data_block.Contains("Gestern"))
            {
                reg_data_full = DateTime.Today.AddDays(-1);
                return reg_data_full;
            }
            else
            {
                reg_data_full = Convert.ToDateTime(data_block);
                return reg_data_full;
            }
        }

        // Получить номер телефона
        static async void get_phone_number(string AdvId, string PhoneHash)
        {
            try
            {
                string link = "https://www.tutti.ch/api/v10/item/phoneview.json?item_id=" + AdvId + "&phone_hash=" + PhoneHash;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                PhoneNumber = Functions.convert_phone(jObject["number"].ToString());
                Console.WriteLine(PhoneNumber);
                if(PhoneNumber[0]=='+'){ return; }
                else
                {
                    if(PhoneNumber[0]=='0')
                    {
                        PhoneNumber = "+41" + PhoneNumber.Substring(1);
                    }
                    else
                    {
                        PhoneNumber = "+41" + PhoneNumber;
                    }
                }
            }
            catch(Exception e){ Console.WriteLine(e); }
        }
    }
}