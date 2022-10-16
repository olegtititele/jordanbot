// using HtmlAgilityPack;
// using Newtonsoft.Json.Linq;

// using PostgreSQL;
// using Proxies;

// namespace Parser
// {
//     public class BazosPl
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
//         private static string platform = "bazos.pl";
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

//         public static void start_parsing(long user_id)
//         {
//             try
//             {
//                 AnnCnd = 0;
//                 PagesPassed = 1;
//                 AdsPassed = 0;
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
//                 Console.WriteLine(generate_link(link, page));
//                 while(true)
//                 {
//                     web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//                     HtmlDocument document = web.Load(generate_link(link, page));
                    
//                     var advertisements = document.DocumentNode.SelectNodes("//div[@class=\"inzeratynadpis\"]//h2[@class=\"nadpis\"]//a");
//                     if(advertisements != null)
//                     {
//                         foreach (HtmlNode advertisement in advertisements)
//                         {
//                             if(AnnCnd < announ_count && DB.GetState(user_id)=="Parser")
//                             {
//                                 AdvLink = "https://" + link.Split("/")[2] + advertisement.GetAttributeValue("href", "");
//                                 Console.WriteLine(AdvLink);

//                                 AdsPassed++;
//                                 DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);

//                                 if(DB.CheckAdvestisement(user_id, AdvLink))
//                                 {
//                                     continue;   
//                                 }
//                                 else
//                                 {
//                                     pars_adv_info(user_id);    
//                                 }

//                             }
//                             else
//                             {
//                                 string state = "StopParser";
//                                 DB.UpdateState(user_id, state);
//                                 DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                                 return;
//                             }
//                         }    
//                     }
//                     else
//                     {
//                         string state = "StopParser";
//                         DB.UpdateState(user_id, state);
//                         DB.UpdateStatistic(user_id, PagesPassed, AdsPassed);
//                         return;
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
//                 return;
//             }
//         }

//         // Сгенерировать ссылку
//         static string generate_link(string link, int page)
//         {
//             string page_link;
//             string ConvertedPage;
//             if(page==1)
//             {
//                 ConvertedPage = "";
//             }
//             else
//             {
//                 int NewPage = page * 20 - 20;
//                 ConvertedPage = NewPage.ToString();
//             }

//             // if(link.Contains("bazos.pl"))
//             // {
//                 if(link[^1]=='/')
//                 {
//                     page_link = link + ConvertedPage;
//                     return page_link;
//                 }
//                 else
//                 {
//                     page_link = link + "/" + ConvertedPage;
//                     return page_link;
//                 }
//             // }
//             // else
//             // {
//             //     page_link = "https://www.bazos.pl/search.php?hledat=" + link + "&hlokalita=&humkreis=125&cenaod=&cenado=&order=&crz=" + ConvertedPage;
//             //     return page_link;
//             // }
//         }

//         static void pars_adv_info(long user_id)
//         {
//             web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//             HtmlDocument adv_html = web.Load(AdvLink);

//             // Дата регистрации объявления
//             try
//             {
//                 AdvReg = Convert.ToDateTime(adv_html.DocumentNode.SelectSingleNode("//span[@class=\"velikost10\"]").InnerText.Split("[")[1].Split("]")[0]);
//             }
//             catch (Exception){ }

//             if(Functions.check_adv_reg_data(adv_reg_data, AdvReg)){ }else{ return; }

//             // Название объявления
//             try
//             {
//                 AdvTitle = adv_html.DocumentNode.SelectSingleNode("//h1[@class=\"nadpisdetail\"]").InnerText;
//             }
//             catch (Exception){ }

//             // Цена объявления
//             try
//             {
//                 var AdvPriceBlock = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"inzeratycena\"]").InnerText.Trim();
//                 AdvPrice = Functions.convert_price(AdvPriceBlock, "zł");
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
//                 AdvLocation = adv_html.DocumentNode.SelectSingleNode("//div[@class=\"inzeratylok\"]").InnerHtml.Replace("<br>", " ");
//             }
//             catch (Exception){ }

//             // Ссылка на продавца, Имя продавца
//             try
//             {
//                 var trs = adv_html.DocumentNode.SelectNodes("//tr");
//                 foreach (var tr in trs)
//                 {
//                     if(tr.InnerHtml.Contains("Imię:"))
//                     {
//                         SellerName = tr.SelectSingleNode("//td[@colspan=\"2\"]").InnerText;
//                         SellerLink = tr.SelectSingleNode("//td[@colspan=\"2\"]//a").GetAttributeValue("href", "");
//                         break;
//                     }
//                 }
//             }
//             catch (Exception){ }

//             // Номер телефона
//             try
//             {
//                 string AdId = AdvLink.Split("ogloszenia/")[1].Split("/")[0];
//                 string AdCategory = AdvLink.Split("ogloszenia/")[0];
//                 string PhoneId = SellerLink.Split("idphone=")[1].Split("&")[0];
//                 var PhoneBlock = get_phone_number(AdId, AdCategory, PhoneId).Result;
//                 if(PhoneBlock=="null")
//                 {
//                     return;
//                 }
//                 else if(PhoneBlock.Contains("+48"))
//                 {
//                     PhoneNumber = Functions.convert_phone(PhoneBlock);
                    
//                 }
//                 else
//                 {
//                     PhoneNumber = "+48" + Functions.convert_phone(PhoneBlock);
//                 }
//             }
//             catch (Exception){ Console.WriteLine("Нет телефона"); return; }

//             if(Functions.check_blacklist_ads(user_id, PhoneNumber, global_blacklist, local_blacklist)){ }else{ return; }

//             // Количество объявлений продавца
//             try
//             {
//                 web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36";
//                 HtmlDocument seller_html = web.Load(SellerLink);
//                 SellerTotalAds = Functions.leave_only_numbers(seller_html.DocumentNode.SelectSingleNode("//div[@class=\"listainzerat inzeratyflex\"]//div[@class=\"inzeratynadpis\"]").InnerText);
//             }
//             catch (Exception){ }

//             if(Functions.check_seller_adv_count(seller_adv_count, SellerTotalAds)){ }else{ return; }

//             // Добавить объявление в бд
//             try
//             {
//                 Functions.add_adv_to_db(user_id, platform, AdvTitle, AdvPrice, AdvReg.ToString("d"), AdvLink, AdvLocation, AdvImage, SellerName, SellerLink, PhoneNumber, SellerTotalAds.ToString(), SellerReg, AdvBusiness, global_blacklist);
//                 AnnCnd++;
//             }
//             catch (Exception){ return; }
//         }

//         // Получить номер телефона
//         static async Task<string> get_phone_number(string AdId, string AdCategory, string PhoneId)
//         {
//             string responseLink = AdCategory + "detailtel.php?idi=" + AdId + "&idphone=" + PhoneId;
//             HttpClient client = new HttpClient();
//             client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML like Gecko) Chrome/32.0.1700.107 Safari/537.36");
//             client.DefaultRequestHeaders.Add("cookie", "__gads=ID=ad0620adc424e6f1-229b385782cd008b:T=1651076364:RT=1651076364:S=ALNI_MafNca6QRloFo1cj0FiIFs9NUE-nw; bkod=F9NX4D9YIL; bid=52675671; __gsas=ID=6dd8a795fed29617:T=1651078105:S=ALNI_MbpS5Ga3R8O_UbnWSEdsXBQYUW5uQ; testcookie=ano");
//             HttpResponseMessage response = await client.GetAsync(responseLink);
//             response.EnsureSuccessStatusCode();
//             string html = await response.Content.ReadAsStringAsync();
    
//             if(html.Contains("Twój numer telefoniczny"))
//             {
//                 return "null";
//             }
//             else
//             {
//                 var htmlDoc = new HtmlDocument();
//                 htmlDoc.LoadHtml(html);
//                 string phone = htmlDoc.DocumentNode.SelectSingleNode("//a").InnerText;
//                 return phone;
//             }
//         }
//     }
// }