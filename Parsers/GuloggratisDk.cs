using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;
using Proxies;

namespace Parser
{
    public class GuloggratisDk
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
        private static string platform = "guloggratis.dk";
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
                    HtmlDocument document = web.Load(generate_link(link, page));
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"wFZ9v6X3JunmCfe_yS_5\"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                            {
                                AdvLink = "https://www.guloggratis.dk" + advertisement.GetAttributeValue("href", "");

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
            if(link.Contains("https://www.guloggratis.dk/"))
            {
                if(link[^1]=='/')
                {
                    page_link = link + "?display=list&order=desc&page=" + page.ToString() + "&sort=updated";
                    return page_link;
                }
                else
                {
                    page_link = link + "/?display=list&order=desc&page=" + page.ToString() + "&sort=updated";
                    return page_link;
                }
            }
            else
            {
                page_link = "https://www.guloggratis.dk/s/q-" + link + "/?display=list&order=desc&page=" + page.ToString() + "&sort=updated";
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            try
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument adv_html = web.Load(AdvLink);
                // Получить json
                try
                {
                    var scripts = adv_html.DocumentNode.SelectNodes("//script");
                    foreach(var script in scripts)
                    {
                        if(script.InnerText.Contains("window['__INITIAL_STATE_GRAPHQL__'] = "))
                        {
                            json = script.InnerText.Split("listing\":")[1].Split(",\"fields\":")[0] + "}";
                            break;
                        }
                    }
                }
                catch(Exception){ return; }

                JObject jObject = JObject.Parse(json);

                // Номер телефона
                try
                {
                    var PhoneBlock = jObject["phones"][0]["text"].ToString();
                    if(PhoneBlock.Contains("+45"))
                    {
                        PhoneNumber = Functions.convert_phone(PhoneBlock);
                        
                    }
                    else
                    {
                        PhoneNumber = "+45" + Functions.convert_phone(PhoneBlock);
                    }
                }
                catch (Exception){ return; }

                if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

                // Тип объявления
                try
                {
                    string TypeAcc = jObject["user"]["isBusiness"].ToString();
                    if(TypeAcc=="False")
                    {
                        AdvBusiness = "Частное лицо";
                    }
                    else
                    {
                        AdvBusiness = "Бизнесс аккаунт";
                    }
                }
                catch(Exception){ AdvBusiness = "Частное лицо"; }

                if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

                // Дата регистрации объявления
                try
                {
                    AdvReg = Convert.ToDateTime(jObject["onlineAt"].ToString());
                }
                catch (Exception){ }

                if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

                // Дата регистации продавца
                try
                {
                    SellerReg = convert_seller_data(jObject["user"]["memberSince"].ToString());
                }
                catch (Exception){ }

                if(Functions.check_seller_reg_data(seller_reg_data, SellerReg)){ }else{ return; }

                // Количество объявлений продавца
                try
                {
                    SellerTotalAds = Int32.Parse(jObject["user"]["onlineListingsCount"].ToString());
                }
                catch (Exception){ }

                if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

                // Название объявления
                try
                {
                    AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"UhCEtHfgPtlwxUcQp9Cm\"]").InnerText;
                }
                catch (Exception){ }

                // Цена объявления
                try
                {
                    var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"jaCBDGMZ3rdGDGePt0GQ\"]").InnerText;
                    AdvPrice = Functions.convert_price(AdvPriceBlock.Substring(0, AdvPriceBlock.Length - 1), "DKK");
                }
                catch (Exception){ }

                // Ссылка на изображение
                try
                {
                    AdvImage = adv_html.DocumentNode.SelectSingleNode("//meta[@property=\"og:image\"]").GetAttributeValue("content", "");
                }
                catch (Exception)
                {
                    AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
                }

                // Местоположение
                try
                {
                    AdvLocation = adv_html.DocumentNode.SelectSingleNode("//span[@class=\"Z8FnXXN9JJ_dmXfjOQez\"]").InnerText;
                }
                catch (Exception){ }

                // Имя продавца
                try
                {
                    SellerName = adv_html.DocumentNode.SelectSingleNode("//a[@class=\"IJ0G4xNMnlA4iH3vD5uW P12gy8aw40HmH6cAqXtM qLAVp3jIHn49JMLO7dTL\"]//h3").InnerText;
                }
                catch (Exception){ }

                // Ссылка на продавца
                try
                {
                    SellerLink = "https://www.guloggratis.dk/ny" + adv_html.DocumentNode.SelectSingleNode("//a[@class=\"IJ0G4xNMnlA4iH3vD5uW P12gy8aw40HmH6cAqXtM qLAVp3jIHn49JMLO7dTL\"]").GetAttributeValue("href", "");
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

        // Форматировать дату регистрации продавца
        static DateTime convert_seller_data(string data_block)
        {
            DateTime reg_data_full;
            int day;
            int month;
            int year;

            if(data_block.Contains("jan"))
            {
                day = Functions.leave_only_numbers(data_block.Split("jan")[0]);
                month = 1;
                year = Functions.leave_only_numbers(data_block.Split("jan")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("feb"))
            {
                day = Functions.leave_only_numbers(data_block.Split("feb")[0]);
                month = 2;
                year = Functions.leave_only_numbers(data_block.Split("feb")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("mar"))
            {
                day = Functions.leave_only_numbers(data_block.Split("mar")[0]);
                month = 3;
                year = Functions.leave_only_numbers(data_block.Split("mar")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            } 
            else if(data_block.Contains("apr"))
            {
                day = Functions.leave_only_numbers(data_block.Split("apr")[0]);
                month = 4;
                year = Functions.leave_only_numbers(data_block.Split("apr")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("maj"))
            {
                day = Functions.leave_only_numbers(data_block.Split("maj")[0]);
                month = 5;
                year = Functions.leave_only_numbers(data_block.Split("maj")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("jun"))
            {
                day = Functions.leave_only_numbers(data_block.Split("jun")[0]);
                month = 6;
                year = Functions.leave_only_numbers(data_block.Split("jun")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("jul"))
            {
                day = Functions.leave_only_numbers(data_block.Split("jul")[0]);
                month = 7;
                year = Functions.leave_only_numbers(data_block.Split("jul")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("aug"))
            {
                day = Functions.leave_only_numbers(data_block.Split("aug")[0]);
                month = 8;
                year = Functions.leave_only_numbers(data_block.Split("aug")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("sep"))
            {
                day = Functions.leave_only_numbers(data_block.Split("sep")[0]);
                month = 9;
                year = Functions.leave_only_numbers(data_block.Split("sep")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("okt"))
            {
                day = Functions.leave_only_numbers(data_block.Split("okt")[0]);
                month = 10;
                year = Functions.leave_only_numbers(data_block.Split("okt")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("nov"))
            {
                day = Functions.leave_only_numbers(data_block.Split("nov")[0]);
                month = 11;
                year = Functions.leave_only_numbers(data_block.Split("nov")[1]);
                reg_data_full = new DateTime(year, month, day);
                return reg_data_full;
            }
            else if(data_block.Contains("dec"))
            {
                day = Functions.leave_only_numbers(data_block.Split("dec")[0]);
                month = 12;
                year = Functions.leave_only_numbers(data_block.Split("dec")[1]);
                reg_data_full = new DateTime(year, month, day);
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