using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class QuokaDe
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
        private static string platform = "quoka.de";
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
        private static string ParsLink = "";


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
                ParsLink = generate_link(link, page);

                while(true)
                {
                    web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                    HtmlDocument document = web.Load(ParsLink);
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"qaheadline item fn\"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                            {
                                AdvLink = "https://www.quoka.de" + advertisement.GetAttributeValue("href", "");
                                Console.WriteLine(AdvLink);

                                AdsPassed++;
                                DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
                                if(DB.CheckAdvestisement(user_id, AdvLink))
                                {
                                    Console.WriteLine("est");
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
                    ParsLink = document.DocumentNode.SelectSingleNode("//link[@rel=\"next\"]").GetAttributeValue("href", "");
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
            if(link.Contains("https://www.quoka.de/"))
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument document = web.Load(link);
                var scripts = document.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    if(script.InnerText.Contains("catid"))
                    {
                        string json = "{" + script.InnerText.Split("({")[1].Split("});")[0] + "}";
                        JObject jObject = JObject.Parse(json);
                        string catid = jObject["catid"].ToString();
                        page_link = "https://www.quoka.de/qmca/search/search.html?redirect=0&catid=" + catid + "&pageno=" + page.ToString();
                        return page_link;
                    }
                }
            }
            else if(link.Contains("https://m.quoka.de/"))
            {
                link = link.Replace("https://m.quoka.de/", "https://www.quoka.de/");
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument document = web.Load(link);
                var scripts = document.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    if(script.InnerText.Contains("catid"))
                    {
                        string json = "{" + script.InnerText.Split("({")[1].Split("});")[0] + "}";
                        JObject jObject = JObject.Parse(json);
                        string catid = jObject["catid"].ToString();
                        page_link = "https://www.quoka.de/qmca/search/search.html?redirect=0&catid=" + catid + "&pageno=" + page.ToString();
                        return page_link;
                    }
                }
            }
            return "";
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);
            

            // Номер телефона
            try
            {
                PhoneNumber = Functions.convert_phone(adv_html.DocumentNode.SelectSingleNode("//ul[@id=\"phoneContactBlock\"]").SelectNodes(".//span")[1].InnerText.Trim());
                if(PhoneNumber.Contains('+'))
                {

                }
                else
                {
                    PhoneNumber = '+' + PhoneNumber;
                }
            }
            catch (Exception){ return; }
            Console.WriteLine(PhoneNumber);

            if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            // Тип объявления
            try
            {
                var TypeAcc = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"date-and-clicks\"]").InnerText;
                Console.WriteLine(TypeAcc);

                
                if(TypeAcc.Contains("privates Angebot")){ AdvBusiness = "Частное лицо"; }
                else{ AdvBusiness = "Бизнесс аккаунт"; }
            }
            catch(Exception){ }

            if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

            // Дата регистации продавца
            try
            {
                var DataBlock = adv_html.DocumentNode.SelectSingleNode("//div[@data-vue-id=\"profile_miniature\"]").GetAttributeValue(":membership", "").Split(" ")[1];
                Console.WriteLine(DataBlock);
                SellerReg = Convert.ToDateTime(DataBlock.Substring(0, DataBlock.Length - 1));
            }
            catch (Exception){ }

            if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }

            // Дата регистрации объявления и ссылка на продавца
            try
            {
                var scripts = adv_html.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    if(script.InnerText.Contains("adReleaseDate"))
                    {
                        string json = "{" + script.InnerText.Split("({")[1].Split("});")[0] + "}";
                        JObject jObject = JObject.Parse(json);
                        AdvReg = Convert.ToDateTime(jObject["adReleaseDate"].ToString());
                        string adPublisher = jObject["adPublisher"].ToString();
                        SellerLink = "https://www.quoka.de/kleinanzeigen/cat_0_cno_" + adPublisher + ".html";
                    }
                }
            }
            catch (Exception){ }

            if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }
            
            // Количество объявлений продавца
            try
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument seller_html = web.Load(SellerLink);
                SellerTotalAds = Functions.leave_only_numbers(seller_html.DocumentNode.SelectSingleNode("//div[@class=\"td n2\"]//h1").InnerText.Trim());
                Console.WriteLine(SellerTotalAds);
            }
            catch (Exception){ }

            if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@itemprop=\"name\"]").InnerText.Trim();
            }
            catch (Exception){ }

            // Цена объявления
            try
            {
                AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//div[@class=\"price has-type\"]//span").InnerText.Trim().Replace(",-", string.Empty), "EUR");
                
            }
            catch (Exception){ }

            // Ссылка на изображение
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode($"//a[@class=\"qx_image-box__trigger\"]//img").GetAttributeValue("src", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }

            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//span[@class=\"locality\"]").InnerText.Trim();
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