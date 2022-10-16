using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;
using Proxies;

namespace Parser
{
    public class AnibisCh
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
        private static string platform = "anibis.ch";
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
        private static string json = "";


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
                    HtmlDocument document = web.Load(generate_link(link, page), ParserProxy.myProxyIP, ParserProxy.myPort, ParserProxy.login, ParserProxy.password);
                    var advertisements = document.DocumentNode.SelectNodes("//a[@type=\"serp\"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)!="StopParser")
                            {
                                AdvLink = "https://www.anibis.ch" + advertisement.GetAttributeValue("href", "");

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
            if(link.Contains("https://www.anibis.ch/"))
            {
                if(link[^1]=='/')
                {
                    page_link = link.Substring(0, link.Length - 1) + "?pi=" + page.ToString();
                    return page_link;
                }
                else
                {
                    page_link = link + "?pi=" + page.ToString();
                    return page_link;
                }
            }
            else
            {
                page_link = "https://www.anibis.ch/fr/c/toutes-les-rubriques?fts=" + link + "&pi=" + page.ToString();
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            try
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument adv_html = web.Load(AdvLink, ParserProxy.myProxyIP, ParserProxy.myPort, ParserProxy.login, ParserProxy.password);

                // Получить json
                try
                {
                    var script = adv_html.DocumentNode.SelectSingleNode("//script[@id=\"state\"]").InnerHtml;
                    json = script.Split("window.__INITIAL_STATE__ = ")[1].Split(",\"breadcrumbs\"")[0].Trim()+"}}";
                }
                catch(Exception){ return; }

                JObject jObject = JObject.Parse(json);

                // Номер телефона
                try
                {
                    var PhoneBlock = jObject["detail"]["contact"]["phone"].ToString();
                    if(PhoneBlock.Contains("41"))
                    {
                        if(PhoneBlock.Contains("+"))
                        {
                            PhoneNumber = Functions.convert_phone(PhoneBlock);
                        }
                        else
                        {
                            PhoneNumber = "+" + Functions.convert_phone(PhoneBlock);
                        }
                        
                    }
                    else
                    {
                        PhoneNumber = "+41" + Functions.convert_phone(PhoneBlock);
                    }
                }
                catch (Exception){ return; }

                if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

                // Тип объявления
                try
                {
                    var TypeAcc = adv_html.DocumentNode.SelectSingleNode("//button[@class=\"max-w-full flex items-center justify-center relative overflow-hidden rounded-4 py-8 px-12 font-semibold text-base focus:outline-0 focus-visible:ring-2 focus-visible:ring-blue-medium cursor-pointer disabled:cursor-auto select-none text-ellipsis transition duration-300 ease-in-out text-blue-primary bg-transparent md:hover:bg-blue-light md:active:bg-blue-light px-1 disabled:text-grey-73 disabled:hover:bg-transparent h-regular\"]//span[@class=\"flex items-center justify-center relative w-full\"]//span[@class=\"whitespace-nowrap overflow-hidden text-ellipsis\"]").InnerHtml;
                    AdvBusiness = "Бизнесс аккаунт";
                }
                catch(Exception){ AdvBusiness = "Частное лицо"; }

                if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

                // Дата регистрации объявления
                try
                {
                    AdvReg = Convert.ToDateTime(jObject["detail"]["formattedModified"].ToString());
                }
                catch (Exception){ }

                if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

                // Дата регистации продавца
                try
                {
                    SellerReg = Convert.ToDateTime(jObject["detail"]["seller"]["registrationDate"].ToString().Split(" ")[^1]);
                }
                catch (Exception){ }

                if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }

                // Количество объявлений продавца
                try
                {
                    SellerTotalAds = Int32.Parse(jObject["detail"]["seller"]["amountOfActiveListings"].ToString());
                }
                catch (Exception){ }

                if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

                // Название объявления
                try
                {
                    AdvTitle = adv_html.DocumentNode.SelectSingleNode("//title").InnerText.Trim();
                }
                catch (Exception){ }

                // Цена объявления
                try
                {
                    var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"sc-16i19r8-0 knSuBJ\"]").InnerText;
                    AdvPrice = Functions.convert_price(AdvPriceBlock.Substring(0, AdvPriceBlock.Length - 2), "CHF");
                }
                catch (Exception){ }

                // Ссылка на изображение
                try
                {
                    AdvImage = adv_html.DocumentNode.SelectSingleNode("//meta[@itemprop=\"image\"]").GetAttributeValue("content", "");
                }
                catch (Exception)
                {
                    AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
                }

                // Местоположение
                try
                {
                    AdvLocation = jObject["detail"]["location"]["country"].ToString() + ", " + jObject["detail"]["location"]["state"].ToString() + ", " + jObject["detail"]["location"]["zipCity"].ToString();
                }
                catch (Exception){ }

                // Имя продавца
                try
                {
                    SellerName = jObject["detail"]["seller"]["name"].ToString();
                }
                catch (Exception){ }

                // Ссылка на продавца
                try
                {
                    SellerLink = "https://www.anibis.ch" + jObject["detail"]["seller"]["allListingsBySellerUrl"].ToString();
                }
                catch (Exception){ }

                // Добавить объявление в бд
                try
                {
                    Functions.InsertNewAd(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg.ToString("d"), AdvBusiness, "0", global_blacklist);
                    AnnCnd++;
                }
                catch (Exception){ return; }
            }
            catch(Exception){ return; }
        }
    }
}