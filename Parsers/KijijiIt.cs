// using HtmlAgilityPack;

// using PostgreSQL;

// namespace Parser
// {
//     public class KijijiIt
//     {
//         private static int AnnCnd;
//         private static string AdvTitle = "Не указано";
//         private static string AdvLink = "";
//         private static string AdvPrice = "Не указана";
//         private static string AdvImage = "";
//         private static string SellerName = "Не указано";
//         private static string SellerLink = "Не указана";
//         private static int SellerTotalAds = 1;
//         private static DateTime AdvReg = DateTime.Today;
//         private static string SellerReg = "Не указана";
//         private static string AdvBusiness = "Частное лицо";
// 		private static string AdvLocation = "Не указано";
//         private static string PhoneNumber = "";
//         private static long user_id;
//         private static string platform = "kijiji.it";
//         private static int page;
//         private static string link = "";
//         private static int announ_count;
//         private static string seller_adv_count="";
//         private static string adv_reg_data = "";
// 		private static string seller_reg_data = "";
//         private static string business = "";
//         private static string local_blacklist = "";
//         private static string global_blacklist = "";
//         private static int PagesPassed = 1;
//         private static int AdsPassed;
//         private static HtmlWeb web = new HtmlWeb();

//         public static int start_parsing(long usid)
//         {
//             try
//             {
//                 user_id = usid;
//                 AnnCnd = 0;
//                 var parameters = DB.GetAllParameters(user_id);
//                 link = parameters[1];
//                 announ_count = Int32.Parse(parameters[2]);
//                 seller_adv_count = parameters[3];
//                 seller_reg_data = parameters[4];
//                 adv_reg_data = parameters[5];
//                 business = parameters[6];
//                 page = Int32.Parse(parameters[7]);
//                 local_blacklist = parameters[8];
//                 global_blacklist = parameters[9];
//                 PagesPassed = 1;
//                 AdsPassed = 0;
//                 while(true)
//                 {
//                     web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//                     HtmlDocument document = web.Load(generate_link(link, page));
//                     var advertisements = document.DocumentNode.SelectNodes("//h3[@class=\"title\"]//a[@class=\"cta\"]");
//                     if(advertisements != null)
//                     {
//                         foreach (HtmlNode advertisement in advertisements)
//                         {
//                             if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
//                             {
//                                 AdvLink = advertisement.GetAttributeValue("href", "");

//                                 AdsPassed++;
//                                 DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                                 if(DB.CheckAdvestisement(user_id, AdvLink))
//                                 {
//                                     continue;   
//                                 }
//                                 else
//                                 {
//                                     pars_adv_info();    
//                                 }

//                             }
//                             else
//                             {
//                                 string state = "StopParser";
//                                 DB.UpdateState(user_id, state);
//                                 DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                                 return 1;
//                             }
//                         }    
//                     }
//                     else
//                     {
//                         string state = "StopParser";
//                         DB.UpdateState(user_id, state);
//                         DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                         return 1;
//                     }
//                     PagesPassed++;
//                     page++;
//                 }
//             }
//             catch(Exception)
//             {
//                 string state = "StopParser";
//                 DB.UpdateState(user_id, state);
//                 DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                 return 1;
//             }
//         }

//         // Сгенерировать ссылку
//         static string generate_link(string link, int page)
//         {
//             string page_link;
//             if(link.Contains("https://www.kijiji.it/"))
//             {
//                 if(link[^1]=='/')
//                 {
//                     page_link = link + "?p=" + page.ToString();
//                     return page_link;
//                 }
//                 else
//                 {
//                     page_link = link + "/?p=" + page.ToString();
//                     return page_link;
//                 }
//             }
//             else
//             {
//                 page_link = "https://www.kijiji.it/" + link +"/?p=" + page.ToString() + "&entryPoint=sb";
//                 return page_link;
//             }
//         }

//         static void pars_adv_info()
//         {
//             web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//             HtmlDocument adv_html = web.Load(AdvLink);

//             // Получить номер телефона
//             try
//             {
//                 PhoneNumber = Functions.convert_phone(adv_html.DocumentNode.SelectSingleNode("//a[@title=\"Chiama\"]").GetAttributeValue("href", ""));
//             }
//             catch(Exception){ return; }

//             if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

//             // Дата регистрации объявления
//             try
//             {
//                 var adv_reg_block = adv_html.DocumentNode.SelectNodes("//span[@class=\"vip__informations__value\"]");
//                 if (adv_reg_block != null)
//                 {
//                     foreach (HtmlNode arb in adv_reg_block)
//                     {
//                         if(arb.InnerText.Contains('/'))
//                         {
//                             AdvReg = Convert.ToDateTime(arb.InnerText);
//                         }
//                     }
//                 }
//             }
//             catch (Exception){ }

//             if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

//             // Тип объявления
//             try
//             {
//                 string seller_info = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"media__body\"]//div[@class=\"subtitle\"]").InnerText.Trim();
//                 if(seller_info.Contains("privato"))
//                 { 
//                     AdvBusiness = "Частное лицо";
//                 }
//                 else
//                 {
//                     AdvBusiness = "Бизнесс аккаунт";
//                 }
//             }
//             catch (Exception){ }

//             if(Functions.check_type_acc(business, AdvBusiness)){ }else{ return; }

//             // Имя и ссылка на продавца
//             try
//             {
//                 SellerName = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"media__body\"]//div[@class=\"title\"]").InnerText.Trim();
//                 SellerLink =  "https://www.kijiji.it" + adv_html.DocumentNode.SelectSingleNode("//span[@class=\"user__otherads mask-linking\"]//div//input").GetAttributeValue("value", "");
//             }
//             catch (Exception){ }

//             // Количество объявлений продавца
//             try
//             {
//                 web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//                 HtmlDocument seller_html = web.Load(SellerLink);
//                 SellerTotalAds = Functions.leave_only_numbers(seller_html.DocumentNode.SelectSingleNode("//h2[@class=\"page-hed\"]").InnerText.Trim());
//             }
//             catch (Exception){ }

//             if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }


//             // Название объявления
//             try
//             {
//                 AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"heading-4 font-weight-regular vip__title\"]").InnerText.Trim();
//             }
//             catch (Exception){ }
            
//             // Цена объявления
//             try
//             {
//                 AdvPrice = Functions.convert_price(adv_html.DocumentNode.SelectSingleNode("//h2[@class=\"heading-4 color-blue-500 font-weight-regular vip__price\"]").InnerText.Trim(), "EUR");
//             }
//             catch (Exception){ }
            
//             // Ссылка на изображение
//             try
//             {
//                 AdvImage = adv_html.DocumentNode.SelectSingleNode($"//img[@alt=\"{AdvTitle}\"]").GetAttributeValue("src", "");
//             }
//             catch (Exception)
//             {
//                 AdvImage = "https://upload.wikimedia.org/wikipedia/commons/9/9a/%D0%9D%D0%B5%D1%82_%D1%84%D0%BE%D1%82%D0%BE.png";
//             }

//             // Местоположение
//             try
//             {
//                 AdvLocation = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"vip__location\"]").InnerText.Trim();
//             }
//             catch (Exception){ }

//             // Добавить объявление в бд
//             try
//             {
//                 Functions.add_adv_to_db(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg, AdvBusiness, global_blacklist);
//                 AnnCnd++;
//             }
//             catch (Exception){ }
//         }
//     }
// }