using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class BazarBg
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
        private static string PhoneNumber = "Не указана";
        private static long user_id;
        private static string platform = "bazar.bg";
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
                    var advertisements = document.DocumentNode.SelectNodes("//a[contains(@class, 'listItemLink  ')]");
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
        static string generate_link(string link, int page)
        {
            string page_link;
            if(business=="Частное лицо")
            {
                if(link.Contains("https://bazar.bg/"))
                {
                    if(link[^1]=='/')
                    {
                        page_link = link.Substring(0, link.Length - 1) + "?filter=1&page=" + page.ToString();
                        return page_link;
                    }
                    else
                    {
                        page_link = link + "?filter=1&page=" + page.ToString();
                        return page_link;
                    }
                }
                else
                {
                    page_link = "https://bazar.bg/obiavi?filter=1&page=" + page.ToString() +"&q=" + link;
                    return page_link;
                }
            }
            else
            {
                if(link.Contains("https://bazar.bg/"))
                {
                    if(link[^1]=='/')
                    {
                        page_link = link.Substring(0, link.Length - 1) + "?filter=all&page=" + page.ToString();
                        return page_link;
                    }
                    else
                    {
                        page_link = link + "?filter=all&page=" + page.ToString();
                        return page_link;
                    }
                }
                else
                {
                    page_link = "https://bazar.bg/obiavi?filter=all&page=" + page.ToString() +"&q=" + link;
                    return page_link;
                }
            }
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);

            // Номер телефона продавца
            try
            {
                string data_id = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"adConnectButton adConnectButtonPhone showPhone\"]").GetAttributeValue("data-id", "");
                string data_hash = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"adConnectButton adConnectButtonPhone showPhone\"]").GetAttributeValue("data-hash", "");
                var PhoneBlock =get_phone_number(data_id, data_hash).Result;
                if(PhoneBlock == "null")
                {
                    return;
                }
                else if(PhoneBlock.Contains("+359"))
                {
                    PhoneNumber = Functions.convert_phone(PhoneBlock);
                }
                else
                {
                    PhoneNumber = "+359" + Functions.convert_phone(PhoneBlock);
                }
            }
            catch (Exception){ return; }

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            // Дата регистрации объявления
            try
            {
                AdvReg = convert_adv_data(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"adDate\"]").InnerText.Trim());
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

            // Дата регистации продавца
            try
            {
                SellerReg = convert_seller_data(adv_html.DocumentNode.SelectSingleNode("//div[@class=\"main\"]//span[@class=\"date\"]").InnerHtml);
            }
            catch (Exception){ }

            if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }
            
            // Количество объявлений продавца
            try
            {
                SellerTotalAds = Functions.leave_only_numbers(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"clever-hide usrAds\"]").InnerText.Trim());
            }
            catch (Exception){ }

            if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"adName \"]").InnerText.Trim();
            }
            catch (Exception){ }
            
            // Цена объявления
            try
            {
                AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"price\"]").InnerText.Trim(), "BGN");
                
            }
            catch (Exception){ }
            
            // Ссылка на изображение
            try
            {
                AdvImage = "https:" + adv_html.DocumentNode.SelectSingleNode($"//img[@class=\"picture\"]").GetAttributeValue("src", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }
            
            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"location\"]").InnerText.Trim();
            }
            catch (Exception){ }

            // Имя продавца
            try
            {
                SellerName = adv_html.DocumentNode.SelectSingleNode("//span[@class=\"clever-hide usrName\"]").InnerText.Trim();
                SellerLink = "https://bazar.bg" + adv_html.DocumentNode.SelectSingleNode("//span[@class=\"clever-hide usrName\"]").GetAttributeValue("data-link", "");
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
        static async Task<string> get_phone_number(string data_id, string data_hash)
        {
            try
            {
                string link = "https://bazar.bg/phone_reveal?id=" + data_id + "&hash=" + data_hash;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                HttpResponseMessage response = await client.GetAsync(link);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                string phone;
                phone = jObject["phone"].ToString();
                return phone;
            }
            catch(Exception)
            {
                string phone ="null";
                return phone;
            }
        }

        // Форматировать дату регистрации продавца
        static DateTime convert_seller_data(string data_block)
        {
            DateTime reg_data_full;
            int day;
            int month;
            int year;

            if(data_block.Contains("януари"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("януари")[0]);
                month = 1;
                year = Functions.leave_only_numbers(data_block.Split("януари")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("февруари"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("февруари")[0]);
                month = 2;
                year = Functions.leave_only_numbers(data_block.Split("февруари")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("март"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("март")[0]);
                month = 3;
                year = Functions.leave_only_numbers(data_block.Split("март")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            } 
            else if(data_block.Contains("април"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("април")[0]);
                month = 4;
                year = Functions.leave_only_numbers(data_block.Split("април")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("май"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("май")[0]);
                month = 5;
                year = Functions.leave_only_numbers(data_block.Split("май")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("юни"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("юни")[0]);
                month = 6;
                year = Functions.leave_only_numbers(data_block.Split("юни")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("юли"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("юли")[0]);
                month = 7;
                year = Functions.leave_only_numbers(data_block.Split("юли")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("август"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("август")[0]);
                month = 8;
                year = Functions.leave_only_numbers(data_block.Split("август")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("септември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("септември")[0]);
                month = 9;
                year = Functions.leave_only_numbers(data_block.Split("септември")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("октомври"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("октомври")[0]);
                month = 10;
                year = Functions.leave_only_numbers(data_block.Split("октомври")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("ноември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("ноември")[0]);
                month = 11;
                year = Functions.leave_only_numbers(data_block.Split("ноември")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("декември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("от")[1].Split("декември")[0]);
                month = 12;
                year = Functions.leave_only_numbers(data_block.Split("декември")[1].Split("г.")[0]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else
            {
                reg_data_full = DateTime.Today;
                return reg_data_full;
            }
        }

        // Форматировать дату регистрации объявления
        static DateTime convert_adv_data(string data_block)
        {
            DateTime reg_data_full;
            int day;
            int month;
            int year = DateTime.Today.Year;

            if(data_block.Contains("днес"))
            {
                reg_data_full = DateTime.Today;
                return reg_data_full;
            }
            else if(data_block.Contains("вчера"))
            {
                reg_data_full = DateTime.Today.AddDays(-1);
                return reg_data_full;
            }
            else if(data_block.Contains("януари"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 1;
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("февруари"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 2;
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("март"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 3;
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            } 
            else if(data_block.Contains("април"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 4;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("май"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 5;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("юни"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 6;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("юли"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 7;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("август"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 8;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("септември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 9;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("октомври"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 10;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("ноември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 11;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else if(data_block.Contains("декември"))
            {
                day = Functions.leave_only_numbers(data_block.Split("на")[1].Split("в")[0]);
                month = 12;
                reg_data_full = new DateTime(year, month, day);
                if(reg_data_full > DateTime.Today)
                {
                    reg_data_full = new DateTime(year-1, month, day);
                }
                return reg_data_full;
            }
            else
            {
                reg_data_full = DateTime.Today;
                return reg_data_full;
            }
        }
    }
}