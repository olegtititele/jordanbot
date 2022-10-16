using HtmlAgilityPack;

using PostgreSQL;

namespace Parser
{
    public class OglasnikHr
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
        private static DateTime SellerReg = AdvReg;
        private static string AdvBusiness = "Частное лицо";
		private static string AdvLocation = "Не указано";
        private static string PhoneNumber = "";
        private static long user_id;
        private static string platform = "oglasnik.hr";
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
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"classified-box classified-box-wide no-hover no-checkbox category-listing clearfix izdvojeno-osnovni\"]");
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
            if(link[^1]=='/')
            {   
                link = link.Substring(0, link.Length - 1); 
                page_link = link + "?page=" + page.ToString();
                return page_link;
            }
            else
            {
                page_link = link + "?page=" + page.ToString();
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);
            // Местоположение и номер телефона продавца
            try
            {
                var AdvBlock = adv_html.DocumentNode.SelectNodes("//div[@class=\"top-details\"]//p");
                if (AdvBlock != null)
                {
                    foreach (HtmlNode ab in AdvBlock)
                    {
                        if(ab.InnerHtml.Contains("Lokacija"))
                        {
                            AdvLocation = ab.InnerText.Trim().Split("&nbsp;")[1].Trim();
                        }
                        else if(ab.InnerHtml.Contains("Telefon"))
                        {
                            PhoneNumber = Functions.convert_phone(ab.SelectSingleNode(".//a").GetAttributeValue("href", ""));
                        }
                    }
                }
                if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }
            }
            catch (Exception){ return; }

            // Дата регистрации объявления
            try
            {
                AdvReg = Convert.ToDateTime(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"ad-meta-single visible-xs visible-sm\"]//b").InnerText.Trim().Split("od ")[1]);
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

            // Cсылка на продавца
            try
            {
                SellerLink =  adv_html.DocumentNode.SelectSingleNode("//div[@class=\"top-details\"]//div//a").GetAttributeValue("href", "");
            }
            catch (Exception){ }

            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument seller_html = web.Load(SellerLink);

            // Количество объявлений продавца
            try
            {
                var SellerTotalAdsBlock = seller_html.DocumentNode.SelectNodes("//ul[@class=\"sidebar-lists depth-0 clickable-drill-down\"]//strong//span");
                if (SellerTotalAdsBlock != null)
                {
                    SellerTotalAds = 0;
                    foreach (HtmlNode stab in SellerTotalAdsBlock)
                    {
                        SellerTotalAds += Functions.leave_only_numbers(stab.InnerText);
                    }
                }
            }
            catch (Exception){ }

            if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Дата регистации продавца
            try
            {
                SellerReg = Convert.ToDateTime(seller_html.DocumentNode.SelectSingleNode("//span[@class=\"djelatnost\"]").InnerText.Trim().Split("od: ")[1]);
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"h2-like no-top-margin color-light-blue\"]").InnerText.Trim();
            }
            catch (Exception){ }
            
            // Цена объявления
            try
            {
                
                AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"price-oglas-details\"]").InnerText.Trim(), "HRK");
                
            }
            catch (Exception){ }
            
            // Ссылка на изображение
            try
            {
                AdvImage = "https://www.oglasnik.hr" + adv_html.DocumentNode.SelectSingleNode($"//ul[@class=\"thumbnails\"]//li").GetAttributeValue("data-carousel-url", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }

            // Имя продавца
            try
            {
                SellerName = seller_html.DocumentNode.SelectSingleNode("//h3[@class=\"no-top-margin\"]").InnerText.Trim();
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
    }
}