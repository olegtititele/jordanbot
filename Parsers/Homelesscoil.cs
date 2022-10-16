using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;


namespace Parser
{
    
    public class HomelessCoIl
    {
        private static int AnnCnd;
        private static string AdvTitle = "Не указано";
        private static string AdvLink = "";
        private static string AdvPrice = "Не указана";
        private static string AdvImage = "";
        private static string SellerName = "Не указано";
        private static string SellerLink = "";
        private static int SellerTotalAds = 1;
        private static DateTime AdvReg = DateTime.Today;
        private static string SellerReg = "Не указана";
        private static string AdvBusiness = "Частное лицо";
		private static string AdvLocation = "Не указано";
        private static string PhoneNumber = "";
        private static long user_id;
        private static string platform = "homeless.co.il";
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

        public static void start_parsing(long usid)
        {
            try
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
                    string responseLink = generate_link(link, page);
                    HtmlNodeCollection advertisements = getAllLinksContent(responseLink).Result;
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                            {
                                AdvLink = "https://www.homeless.co.il" + advertisement.SelectSingleNode(".//a[contains(@id, 'ctl00')]").GetAttributeValue("href", "");

                                AdsPassed++;
                                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);

                                if(DB.CheckAdvestisement(user_id, AdvLink))
                                {
                                    continue;   
                                }
                                else
                                {
                                    try
                                    {
                                        var tds = advertisement.SelectNodes(".//td[@style=\"width:150px\"]");
                                        foreach(HtmlNode td in tds)
                                        {
                                            if(CheckDate(td.InnerText))
                                            {
                                                AdvReg = Convert.ToDateTime(td.InnerText);
                                                break;
                                            }
                                        }
                                        if(Functions.check_adv_reg_data(adv_reg_data, AdvReg))
                                        { 
                                            pars_adv_info(); 
                                        }
                                        else{ continue; }
                                    }
                                    catch(Exception)
                                    {
                                        if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ continue; }
                                        pars_adv_info(); 
                                    }
                                }

                            }
                            else
                            {
                                string state = "StopParser";
                                DB.UpdateState(user_id, state);
                                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                                return;
                            }
                        }    
                    }
                    else
                    {
                        string state = "StopParser";
                        DB.UpdateState(user_id, state);
                        DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                        return;
                    }
                    PagesPassed++;
                    page++;
                }
            }
            catch(Exception)
            {
                string state = "StopParser";
                DB.UpdateState(user_id, state);
                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                return;
            }
        }

        
        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);
            // Номер телефона продавца
            try
            {
                string boardType = adv_html.DocumentNode.SelectSingleNode("//button[@class=\"btnDisplayPhone\"]").GetAttributeValue("data-boardtype", "");
                string AdID = adv_html.DocumentNode.SelectSingleNode("//button[@class=\"btnDisplayPhone\"]").GetAttributeValue("data-id", "");
                string PhoneBlock = get_phone_number(boardType, AdID).Result;

                if(PhoneBlock == "null")
                {
                    return;
                }
                else if(PhoneBlock.Contains("+972"))
                {
                    PhoneNumber = Functions.convert_phone(PhoneBlock);
                }
                else
                {
                    PhoneNumber = "+972" + Functions.convert_phone(PhoneBlock);
                }
            }
            catch(Exception){ }

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"right\"]//h1").InnerText;
            }
            catch(Exception){ }

            // Цена объявления
            try
            {
                AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//div[@class=\"price\"]").InnerText, "ILS");
            }
            catch(Exception){ }
            
            // Местоположение
            try
            {
                var adv_location_block = adv_html.DocumentNode.SelectSingleNode("//div[contains(@id, 'ctl00_ContentPlaceHolder1_MainDetails_AdPanel')]").SelectNodes(".//div[contains(@style, 'float:right')]");

                foreach (HtmlNode adv_info in adv_location_block)
                {
                    if(adv_info.InnerText.Contains("עיר"))
                    {
                        AdvLocation = adv_info.SelectSingleNode(".//span").InnerHtml;
                    }
                }
            }
            catch(Exception){ }
            
            // Имя продавца
            try
            {
                SellerName = adv_html.DocumentNode.SelectSingleNode("//div[contains(@class, 'EnableLongContent')]//span").InnerText;
            }
            catch(Exception){ }
        
            // Изображение объявления
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode("//div[@id=\"pic1\"]//img").GetAttributeValue("src", "");
            }
            catch(Exception){ }

            // Добавить объявление в бд
            try
            {
                Functions.InsertNewAd(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg, AdvBusiness, "0", global_blacklist);
                AnnCnd++;
            }
            catch (Exception){ }
        }

        // Сгенерировать ссылку
        static string generate_link(string link, int page)
        {
            string page_link;
            if(link.Contains("m.homeless.co.il"))
            {
                link = link.Replace("m.homeless.co.il", "www.homeless.co.il");
                if(link[^1]=='/')
                {
                    page_link = link + page.ToString();
                    return page_link;
                }
                else
                {
                    page_link = link + "/" + page.ToString();
                    return page_link;
                }
            }
            else
            {
                if(link[^1]=='/')
                {
                    page_link = link + page.ToString();
                    return page_link;
                }
                else
                {
                    page_link = link + "/" + page.ToString();
                    return page_link;
                }
            }
        }

        static async Task<HtmlNodeCollection> getAllLinksContent(string responseLink)
        {   
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:97.0) Gecko/20100101 Firefox/97.0");
            client.DefaultRequestHeaders.Add("Cookie", "ASP.NET_SessionId=4y3qfs45csk311ux5ylnecie; _gcl_au=1.1.1899744802.1654024883; _ga_C4PHDGEWPC=GS1.1.1654080595.5.1.1654085230.0; _ga=GA1.3.690769698.1654024883; _gid=GA1.3.1698604562.1654024884; _fbp=fb.2.1654024884010.1428194505; search_inumber3%3d48_yad2=%7b%22boardtype%22%3a%22yad2%22%2c%22inumber3%22%3a%2248%22%7d; poptin_old_user=true; poptin_user_id=0.89dcj7seh8i; poptin_user_ip=217.66.158.172; poptin_user_country_code=false; poptin_session_account_1b368ab85dea2=true; poptin_c_visitor=true; search_inumber3%3d231_pets=%7b%22boardtype%22%3a%22pets%22%2c%22inumber3%22%3a%22231%22%7d; __atuvc=9%7C22; poptin_referrer=; search_inumber3%3d49_yad2=%7b%22boardtype%22%3a%22yad2%22%2c%22inumber3%22%3a%2249%22%7d; search_inumber3%3d57_yad2=%7b%22boardtype%22%3a%22yad2%22%2c%22inumber3%22%3a%2257%22%7d; __atuvs=62974cbc9f3c3869004; search_inumber3%3d44_yad2=%7b%22boardtype%22%3a%22yad2%22%2c%22inumber3%22%3a%2244%22%7d; search_icompanyid%3d99_special=%7b%22boardtype%22%3a%22special%22%2c%22icompanyid%22%3a%2299%22%7d; search_icompanyid%3d118%2c117_accessories=%7b%22boardtype%22%3a%22accessories%22%2c%22icompanyid%22%3a%22118%2c117%22%7d;_gat_gtag_UA_228947_1=1;poptin_o_v_99d46df4d414e=36462c54e2976; poptin_session=true");
            HttpResponseMessage response = await client.GetAsync(responseLink);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);

            HtmlNodeCollection advertisements = htmlDoc.DocumentNode.SelectNodes("//tr[@type=\"ad\"]");
            return advertisements;
        }

        private static bool CheckDate(string date)
        {
            DateTime dt;
            return DateTime.TryParse(date, out dt);
        }

        static async Task<string> get_phone_number(string boardType, string AdID)
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "boardType", boardType },
                    { "AdID", AdID }
                };

                var data = new FormUrlEncodedContent(values);

                var url = "https://www.homeless.co.il/webservices/icardos.asmx/IncrementClickesAndGetPhoneNumber";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:97.0) Gecko/20100101 Firefox/97.0");

                var response = await client.PostAsync(url, data);
                string json = await response.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(json);
                string phone = htmlDoc.DocumentNode.InnerText.Split(",")[0];
                return phone;
            }
                
            catch(Exception)
            { 
                string phone ="null";
                return phone;
            }
        }
    }
}