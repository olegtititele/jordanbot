using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Globalization;
using PostgreSQL;

namespace Parser
{
    public class JofogasHu
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
        private static DateTime SellerReg = DateTime.Today;
        private static string AdvBusiness = "Частное лицо";
		private static string AdvLocation = "Не указано";
        private static string PhoneNumber = "";
        private static long user_id;
        private static string platform = "jofogas.hu";
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
                    web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                    HtmlDocument document = web.Load(generate_link(link, page));
                    var advertisements = document.DocumentNode.SelectNodes("//h3[@class=\"item-title\"]//a[@class=\"subject \"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                            {
                                AdvLink = advertisement.GetAttributeValue("href", "");

                                AdsPassed++;
                                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                                if(DB.CheckAdvestisement(user_id, AdvLink))
                                {
                                    continue;   
                                }
                                else
                                {
                                    pars_adv_info();    
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
            catch(Exception)
            {
                string state = "StopParser";
                DB.UpdateState(user_id, state);
                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                return 1;
            }
        }

        // Сгенерировать ссылку
        public static string generate_link(string link, int page)
        {
            string page_link;
            if(link.Contains("https://www.jofogas.hu/"))
            {
                page_link = link + "?o=" + page.ToString();
                return page_link;
            }
            else
            {
                page_link = "https://www.jofogas.hu/magyarorszag?q=" + link + "&o=" + page.ToString();
                return page_link;
            }
        }

        public static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);

            // Получить номер телефона
            try
            {
                var adv_id = Functions.leave_only_numbers(adv_html.DocumentNode.SelectSingleNode("//span[@id=\"vi_recommendation\"]").InnerText);
                var PhoneBlock = get_phone_number(adv_id.ToString()).Result;
                if(PhoneBlock == "null")
                {
                    return;
                }
                else if(PhoneBlock.Contains("+36"))
                {
                    PhoneNumber = Functions.convert_phone(PhoneBlock);
                }
                else
                {
                    PhoneNumber = "+36" + Functions.convert_phone(PhoneBlock);
                }
            }
            catch(Exception){ return; }

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            try
            {
                var adv_reg_block = adv_html.DocumentNode.SelectNodes("//script");
                if (adv_reg_block != null)
                {
                    foreach (HtmlNode arb in adv_reg_block)
                    {
                        if(arb.InnerText.Contains("var utag_data"))
                        {
                            // Дата регистрации объявления
                            try
                            {
                                AdvReg = Convert.ToDateTime(arb.InnerHtml.Split("date : \"")[1].Split("\"")[0]);
                            }
                            catch(Exception){ }
                            // Тип объявления
                            try
                            {
                                string seller_info = arb.InnerHtml.Split("acmh_publisher_type : \"")[1].Split("\"")[0];
                                if(seller_info.Contains("private"))
                                { 
                                    AdvBusiness = "Частное лицо";
                                }
                                else
                                {
                                    AdvBusiness = "Бизнесс аккаунт";
                                }
                            }
                            catch (Exception){ }
                        }
                        // Имя и ссылка на продавца
                        else if(arb.InnerText.Contains("advertiser:"))
                        {
                            SellerName = convert_seller_name(arb.InnerText.Split("'name':")[1].Split(",")[0]);
                            SellerLink =  arb.InnerText.Split("'afsuUrl':")[1].Split(",")[0].Split("\"")[1].Split("\"")[0].Trim();
                        }
                    }
                }
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg))
            {
                if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }
            }
            else{ return; }

            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument seller_html = web.Load(SellerLink);

            // Количество объявлений продавца
            try
            {
                SellerTotalAds = Functions.leave_only_numbers(seller_html.DocumentNode.SelectSingleNode("//div[@class=\"col-xs-12 col-md-7 afsu-tabs uppercase\"]").InnerText.Trim());
            }
            catch (Exception){ }

            if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Дата регистации продавца
            try
            {
                SellerReg = Convert.ToDateTime(seller_html.DocumentNode.SelectSingleNode("//div[@class=\"register-date\"]//strong").InnerText.Trim());
            }
            catch (Exception){ }

            if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//title").InnerHtml;
            }
            catch (Exception){ }
            

            // Цена объявления
            try
            {
                AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"price-value\"]").InnerText.Trim(), "HUF");
                
            }
            catch (Exception){ }
            
            // Ссылка на изображение
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode($"//picture//img").GetAttributeValue("src", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }

            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//span[@itemprop=\"addressLocality\"]").InnerText;
            }
            catch (Exception){ }
            
            // Добавить объявление в бд
            try
            {
                Functions.InsertNewAd(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg.ToString("d"), AdvBusiness, "0", global_blacklist);
                AnnCnd++;
            }
            catch (Exception){ }
        }


        // Получить номер телефона
        static async Task<string> get_phone_number(string adv_id)
        {
            try
            {
                string link = "https://apiv2.jofogas.hu/v2/items/getPhone?list_id=" + adv_id;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                client.DefaultRequestHeaders.Add("api_key", "jofogas-web-eFRv9myucHjnXFbj");
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                string phone = jObject["phone"].ToString();
                return phone;
            }
            catch(Exception)
            {
                string phone ="null";
                return phone;
            }
        }

        static string convert_seller_name(string seller_block)
        {
            string seller_name = "";
            for (int i = 0; i < seller_block.Length; i++)
            {
                if (Char.IsLetter(seller_block[i]))
                {
                    seller_name += seller_block[i];
                }
                else
                {
                    continue;
                }
            }
            return seller_name;
        }
    }
}