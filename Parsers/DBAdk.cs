using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class DbaDk
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
        private static string platform = "dba.dk";
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
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"thumbnailContainerInner\"]");
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
            if(link.Contains("https://www.dba.dk/"))
            {
                if(link[^1]=='/')
                {
                    page_link = link + "side-" + page.ToString() + "/";
                    return page_link;
                }
                else
                {
                    page_link = link + "/side-" + page.ToString() + "/";
                    return page_link;
                }
                
            }
            else
            {
                
                page_link = "https://www.dba.dk/soeg/side-"+ page.ToString() +"/?soeg=" + link;
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);


            // Получить номер телефона
            try
            {
                var HavePhone = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"ContactButton_contact-button__1OLA9\"]").InnerHtml;
                var PhoneBlock = get_phone_number().Result;
                if(PhoneBlock == "null")
                {
                    return;
                }
                else if(PhoneBlock.Contains("+45"))
                {
                    PhoneNumber = Functions.convert_phone(PhoneBlock);
                }
                else
                {
                    PhoneNumber = "+45" + Functions.convert_phone(PhoneBlock);
                }
            }
            catch(Exception ){ return; }

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            // Проверка типа объявления
            try
            {
                var AdvBusinessBlock = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"IconTextViewer_icon-text-viewer__R_iD0\"]/p[@class=\"dba-MuiTypography-root dba-MuiTypography-body1\"]").InnerText.Trim();
                if(AdvBusinessBlock.Contains("CVR registreret"))
                {
                    AdvBusiness = "Бизнесс аккаунт";
                }
                else
                {
                    AdvBusiness = "Частное лицо";
                }
            }
            catch (Exception){ }

            if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

            // Дата регистрации объявления, Дата регистрации продавца
            try
            {
                var scripts = adv_html.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    if(script.InnerText.Contains("createdDate"))
                    {
                        string json = script.InnerText.Split("var _profile = ")[1];
                        json = json.Substring(0, json.Length - 1);
                        JObject jObject = JObject.Parse(json);
                        AdvReg = Convert.ToDateTime(jObject["createdDate"]["value"].ToString());
                        SellerReg = Convert.ToDateTime(jObject["profile"]["registrationDate"]["value"].ToString());
                        break;
                    }
                }
            }
            catch(Exception){ }



            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg))
            {
                if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }
            }
            else
            { 
                return; 
            }

            // Имя и ссылка на продавца
            try
            {
                SellerName = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"dba-MuiTypography-root dba-MuiLink-root dba-MuiLink-underlineHover ProfileLink_profile-link__RSMT_ dba-MuiTypography-colorPrimary\"]/h1").InnerText.Trim().Replace("\0", string.Empty);
                SellerLink = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"dba-MuiTypography-root dba-MuiLink-root dba-MuiLink-underlineHover ProfileLink_profile-link__RSMT_ dba-MuiTypography-colorPrimary\"]").GetAttributeValue("href", "").Replace("\0", string.Empty);
            }
            catch (Exception){ }

            // Количество объявлений продавца
            try
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument seller_html = web.Load(SellerLink);
                var scripts = seller_html.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    // Console.WriteLine(script.InnerText);
                    if(script.InnerText.Contains("registrationDate"))
                    {
                        string json = script.InnerText.Split("var _props = ")[1];
                        json = json.Substring(0, json.Length - 1);
                        JObject jObject = JObject.Parse(json);
                        var ads = jObject["listings"]["listings"];
                        SellerTotalAds = 0;
                        foreach(var ad in ads)
                        {
                            SellerTotalAds += 1;
                        }
                    }
                }
            }
            catch (Exception){ }
            
            if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"row-fluid\"]/h1").InnerText.Trim().Replace("\0", string.Empty);
            }
            catch (Exception){ }

            // Цена объявления
            try
            {
                var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//span[@class=\"price-tag\"]").InnerText.Trim();
                AdvPrice = Functions.convert_price(AdvPriceBlock.Substring(0, AdvPriceBlock.Length - 1), "DKK");
            }
            catch (Exception){ }

            // Ссылка на изображение
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"primary svg-placeholder\"]").GetAttributeValue("style", "").Split("(")[1].Split(")")[0].Replace("\0", string.Empty);
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }

            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"dba-MuiTypography-root dba-MuiLink-root dba-MuiLink-underlineHover dba-MuiTypography-colorPrimary\"]/p[@class=\"dba-MuiTypography-root dba-MuiTypography-body1\"]").InnerText.Trim().Replace("\0", string.Empty);
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
        static async Task<string> get_phone_number()
        {
            string responseLink = "https://www.dba.dk/api/dba-vip-site/phonenumber/" + AdvLink.Split("/")[^2].Split("id-")[1];
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:97.0) Gecko/20100101 Firefox/97.0");
            HttpResponseMessage response = await client.GetAsync(responseLink);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);
            string phone;
            try
            {
                phone = jObject["data"][0].ToString();
                return phone;
            }
            catch(Exception)
            { 
                phone ="null";
                return phone;
            }
        }
    }
}