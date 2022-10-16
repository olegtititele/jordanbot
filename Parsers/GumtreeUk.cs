using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

using PostgreSQL;

namespace Parser
{
    public class GumtreeUk
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
                    Console.WriteLine(generate_link(link, page));
                    var advertisements = document.DocumentNode.SelectNodes("//a[@class=\"listing-link \"]");
                    if(advertisements != null)
                    {
                        foreach (HtmlNode advertisement in advertisements)
                        {
                            if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
                            {
                                AdvLink = "https://www.gumtree.com" + advertisement.GetAttributeValue("href", "");
                                Console.WriteLine(AdvLink);

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
            if(link.Contains("https://www.gumtree.com/"))
            {
                if(link[^1]=='/')
                {
                    page_link = link + "uk/page" + page.ToString();
                    return page_link;
                }
                else
                {
                    page_link = link + "/uk/page" + page.ToString();
                    return page_link;
                }
                
            }
            else
            {
                page_link = "https://www.gumtree.com/search?search_category=for-sale&search_location=uk&q="+ link +"&page=" + page.ToString();
                return page_link;
            }
        }

        static void pars_adv_info()
        {
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
            HtmlDocument adv_html = web.Load(AdvLink);

            // Получить json
            try
            {
                var scripts = adv_html.DocumentNode.SelectNodes("//script");
                foreach(var script in scripts)
                {
                    if(script.InnerText.Contains("window.clientData = "))
                    {
                        var JsonBlock = script.InnerText.Split("window.clientData = ")[1];
                        json = JsonBlock.Substring(0, JsonBlock.Length - 1);
                    }
                }
            }
            catch(Exception){ return; }

            JObject jObject = JObject.Parse(json);

            // Получить номер телефона
            try
            {
                var HavePhone = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"seller-phone-reveal\"]").InnerHtml;
                string PhoneToken = jObject["sellerContactDetails"]["revealSellerTelephoneNumberToken"].ToString();
                string AdId = jObject["adDetails"]["id"].ToString();
                var PhoneBlock = get_phone_number(AdId, PhoneToken).Result;
                Console.WriteLine(PhoneBlock);
                if(PhoneBlock == "null")
                {
                    return;
                }
                else if(PhoneBlock.Contains("+44"))
                {
                    PhoneNumber = Functions.convert_phone(PhoneBlock);
                }
                else
                {
                    PhoneNumber = "+44" + Functions.convert_phone(PhoneBlock);
                }
            }
            catch(Exception ){ return; }

            // if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

            // Проверка типа объявления
            try
            {
                var AdvBusinessBlock = jObject["adDetails"]["pro"].ToString();
                if(AdvBusinessBlock=="False")
                {
                    AdvBusiness = "Частное лицо";
                }
                else
                {
                    AdvBusiness = "Бизнесс аккаунт";
                }
            }
            catch (Exception){ }

            // if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

            // Дата регистрации объявления
            try
            {
                var script = adv_html.DocumentNode.SelectSingleNode("//script").InnerHtml.Split("permutive.addon('web', ")[1].Split(");")[0];
                JObject AdvRegObj = JObject.Parse(script);
                AdvReg = Convert.ToDateTime(AdvRegObj["page"]["ad"]["creationDate"].ToString());
            }
            catch(Exception){ }

            // Дата регистрации продавца
            try
            {
                SellerReg = convert_seller_reg(adv_html.DocumentNode.SelectSingleNode("//strong[@class=\"seller-stats-emphasis\"]").InnerText);
            }
            catch(Exception){ }

            // Имя и ссылка на продавца
            try
            {
                SellerName = adv_html.DocumentNode.SelectSingleNode("//h2[@class=\"truncate-line seller-rating-block-name\"]").InnerText;
                SellerLink = "https://www.gumtree.com" + adv_html.DocumentNode.SelectSingleNode("//div[@class=\"css-16tsqsb e4jfc8t14\"]//a[@class=\"link\"]").GetAttributeValue("href", "");
            }
            catch (Exception){ }

            // Количество объявлений продавца
            try
            {
                web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
                HtmlDocument seller_html = web.Load(SellerLink);
                SellerTotalAds = Functions.leave_only_numbers(seller_html.DocumentNode.SelectSingleNode("//div[@class=\"live-ads-profile\"]//h2").InnerText);
            }
            catch (Exception){ }
            
            // if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

            // Название объявления
            try
            {
                AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"css-4rz76v e1pt9h6u6\"]").InnerText;
            }
            catch (Exception){ }

            // Цена объявления
            try
            {
                var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//h3[@itemprop=\"price\"]").GetAttributeValue("content", "");
                AdvPrice = Functions.convert_price(AdvPriceBlock, "GBP");
            }
            catch (Exception){ }

            // Ссылка на изображение
            try
            {
                AdvImage = adv_html.DocumentNode.SelectSingleNode($"//img[@alt=\"{AdvTitle}\"]").GetAttributeValue("src", "");
            }
            catch (Exception)
            {
                AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
            }

            // Местоположение
            try
            {
                AdvLocation = adv_html.DocumentNode.SelectSingleNode("//h4[@itemprop=\"addressLocality\"]").InnerText;
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
        static async Task<string> get_phone_number(string AdId, string PhoneToken)
        {
            try
            {
                string responseLink = "https://www.gumtree.com/ajax/account/seller/reveal/number/" + AdId;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/32.0.1700.107 Safari/537.36");
                client.DefaultRequestHeaders.Add("x-gumtree-token", PhoneToken);
                client.DefaultRequestHeaders.Add("cookie", "gt_p=id:YWUzN2M2ZjUtNDZmZC00OWVhLWJlNTEtZTMxY2ZkMTdkYTlk; gt_ab=ln:MTl3eDM=; GCLB=CIbh1Je5752rQg; _ga=GA1.2.1457986689.1650825049; _gid=GA1.2.186046165.1650825049; _pubcid=eca9cdd7-0b75-4b99-adb4-56f255900528; _lr_env_src_ats=false; OptanonAlertBoxClosed=2022-04-24T18:31:06.411Z; permutive-id=819f2ca7-ff44-4c95-bdcf-43d991e0ca8d; eupubconsent-v2=CPX9-m6PX9-m6AcABBENCMCsAP_AAAAAAAYgIGNf_X__b3_j-_59f_t0eY1P9_7_v-0zjhfdt-8N2f_X_L8X_2M7vF36pq4KuR4Eu3LBIQVlHOHcTUmw6okVrTPsbk2Mr7NKJ7PEmnMbO2dYGH9_n93T-ZKY7_____77_v-_______f__-_f___5_3---_f_V_99zbn9__3__9wAAAPAAAAkEgngAIAAXABQAFQAMgAcAA8ACAAGEANAA1AB5AEMARQAmABPACqAFgAN4AcwA9ACEAENAIgAiYBLAEuAJoAUoAtwBhgDIAGqANkAd4A9gB8QD7AP0AgEBFwEYAI0ARwAlIBQQCngFXALmAYoA1gBtADcAHEAPQAh0BIgCYgEygJsATsAocBSICxQFsALkAXeAvMBgwDCQGGgMiAZIAycBlwDOQGfANIAadA1gDWQG6wORA5UBy4DrAHjgPlCADgBzAGEAU-AyYB0gDsAHZgO6AeAA8oB7QD3QHyAPsDQGwAuACGAGQANkAfgBAACMAFPAKvAWgBaQDWAIdASIAmwBOwCkQFyAMJAYwAycBnIDPAGfAOSAcoA6wB-AYAGAOYB2YD3RAAQAGoA5gHZgPdEQFwBDADIAGyAPwAgABGACngFXANYAh0BIgCbAE7AKRAXIAwkBk4DOQGfAOSAcoA6wB-AqAuABQAIYATAAuAD8AIwARwAq8BaAFpASCAmIBNgCmwFsALkAXmAyIBnIDPAGfAOSAcoA_AUADAHMAPAB9gyAqABQAIYATIB9gH4ARgAjgBVwCtgJiATYAtEBbAC8wGRAM5AZ4Az4ByQDlAHxAPwGABAAagDmAHgA-wdBoAAXABQAFQAMgAcABAAC6AGAAYwA0ADUAHgAPoAhgCKAEwAJ4AVQAsABcAC-AGIAMwAbwA5gB6AENAIgAiYBLAEwAJoAUYApQBYgC3gGEAYYAyABlADRAGyAN8Ad4A9oB9gH6AP-AiwCMAEcgJSAlQBQQCngFXALFAWgBaQC5gF5AMUAbQA3ABxADpgHoAQ6Ai8BIICRAEqAJsATsAocBTQCrAFigLYAXAAuQBdoC7wF5gMGAYSAw0BiQDGAGPAMkAZOAyoBlgDLgGcgM-AaJA0gDSQGlgNOAaqA1gBsYDdQHFwOSA5UBy4DrAHjgPSAeqA-UB9YD8BwA4AcwBhAGTANsAcgA6QB2ADswHgAPKAe0A90B8QD7CEDsABYAFAAMgAuABiAEMAJgAVQAuABfADEAGYAN4AegBYgDCAG-AO-AfYB-AD_AIwARwAlIBQQChgFPAKvAWgBaQC5gGKANoAegBIICRAEqAJsAU0AsUBaIC2AFtALgAXIAu0BiQDIgGTgM5AZ4Az4BogDSQGlgNVAcAA5IB1gDxwH4EABQA5gB4AGEAbYA7AB5QD0QHugPiAfYSgZAAIAAWABQADIAHIAYABiADwAIgATAAqgBcAC-AGIAMwAhoBEAESAKMAUoAtwBhADVAGyAO8AfgBGACOAFPAKvAWgBaQDFAG4AQ6Ai8BIgCbAFigLYAXaAvMBkQDJwGWAM5AZ4Az4BpADWAHAAOsAfgSADADmAdIA7AB5QD2gH2FIJYAC4AKAAqABkADgAIIAYABjADQANQAeQBDAEUAJgATwApABVACwAF8AMQAZgA5gCGgEQARIAowBSgCxAFuAMIAZQA0QBqgDZAHfAPsA_QCLAEYAI4ASkAoIBQwCrgFbALmAXkA2gBuAD0AIdAReAkQBNgCdgFDgLFAWwAuABcgC7QF5gMNAYwAyIBkgDJwGXAM5AZ4Az6BpAGkwNYA1kBsYDdYHJgcoA5cB1gDxwHygPwKADABzADwAMIAp8BkwDsAHZgPKAe0A90B8QD7AAAA.f_gAAAAAAAAA; _fbp=fb.1.1650825074059.799652221; ki_r=; gt_tm=9d6cf4e5-ca02-45a9-b6d7-fa0545b8a1a1; gt_rememberMe=GxCVluBQQiIqIBAi9G3/jPJNh1+iQRP/VcDYyw9sOtUScy2a73ACiXypDfaKtZ78tD6zKZKb1+WDWazq8qE6SLrZhcK2wp1mN/dvSmGb5y9yMMXXS5lurL8hip//xcwzmCacT4Mh2SCv0WwB929nTRl+mxKiS/n0y02wc9/hEYY=; gt_mc=rcd:MA==|nuc:MA==; gt_adconsent=state:Mw==; _lr_geo_location=RU; _lr_drop_match_pixel=true; eCG_eh=ec=ResultsBrowse:ea=NavL1CategoryHoover:el=;");
                HttpResponseMessage response = await client.GetAsync(responseLink);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                JObject jObject = JObject.Parse(json);
                string phone = jObject["data"].ToString();
                return phone;
            }
            catch(Exception)
            { 
                string phone ="null";
                return phone;
            }
        }

        // Форматировать дату регистрации продавца
        static DateTime convert_seller_reg(string DataBlock)
        {
            int RegDataDigit = Functions.leave_only_numbers(DataBlock);
            DateTime reg_data_full;

            if(DataBlock.Contains("day"))
            {
                reg_data_full = DateTime.Today.AddDays(-RegDataDigit);
                return reg_data_full;
            }
            else if(DataBlock.Contains("month"))
            {
                reg_data_full = DateTime.Today.AddDays(-RegDataDigit*30);
                return reg_data_full;
            }
            else if(DataBlock.Contains("year"))
            {
                reg_data_full = DateTime.Today.AddDays(-RegDataDigit*365);
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