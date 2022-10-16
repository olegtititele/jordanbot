using PostgreSQL;
using Parser;
using Telegram.Bot;

namespace Modules
{
    public static class Platforms
    {
        public static void GetParser(string platform, ITelegramBotClient botClient, long chatId, int messageId)
        {
            DB.ClearHash(chatId);
            Thread load = new Thread(()=>Loading.DynamicLoading(botClient, chatId, messageId));
            switch(platform)
            {
                case "homeless.co.il":
                    Thread pars = new Thread(()=>HomelessCoIl.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "dba.dk":
                    pars = new Thread(()=>DbaDk.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                // case "ebay-kleinanzeigen.de":
                //     pars = new Thread(()=>EbayKleinanzeigen.start_parsing(chatId));
                //     pars.Start();
                //     load.Start();
                //     break;
                case "bolha.com":
                    pars = new Thread(()=>BolhaCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "sbazar.cz":
                    pars = new Thread(()=>SBazarCz.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                // case "kijiji.it":
                //     pars = new Thread(()=>KijijiIt.start_parsing(chatId));
                //     pars.Start();
                //     load.Start();
                //     break;
                case "jofogas.hu":
                    pars = new Thread(()=>JofogasHu.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "oglasnik.hr":
                    pars = new Thread(()=>OglasnikHr.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "tutti.ch":
                    pars = new Thread(()=>TuttiCh.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "bazar.bg":
                    pars = new Thread(()=>BazarBg.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "quoka.de":
                    pars = new Thread(()=>QuokaDe.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "anibis.ch":
                    pars = new Thread(()=>AnibisCh.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "gumtree.uk":
                    pars = new Thread(()=>GumtreeUk.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "guloggratis.dk":
                    pars = new Thread(()=>GuloggratisDk.start_parsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "bazar.sk":
                    pars = new Thread(()=>BazarSk.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                // case "bazos.pl":
                //     pars = new Thread(()=>BazosPl.start_parsing(chatId));
                //     pars.Start();
                //     load.Start();
                //     break;
                case "olx.qa":
                    pars = new Thread(()=>ArabOLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "olx.com.om":
                    pars = new Thread(()=>ArabOLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "olx.ro":
                    pars = new Thread(()=>OLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "olx.pt":
                    pars = new Thread(()=>OLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "olx.pl":
                    pars = new Thread(()=>OLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "olx.bg":
                    pars = new Thread(()=>OLX.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "list.am":
                    pars = new Thread(()=>ListAm.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "kijiji.ca":
                    pars = new Thread(()=>KijijiCa.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "lajumate.ro":
                    pars = new Thread(()=>LajumateRo.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "om.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "ye.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "ae.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "iq.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "eg.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "lb.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "ly.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "sa.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "sd.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "bh.opensooq.com":
                    pars = new Thread(()=>OpensooqCom.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
                case "kuldnebors.ee":
                    pars = new Thread(()=>KuldneborsEe.StartParsing(chatId));
                    pars.Start();
                    load.Start();
                    break;
            }
        }
    }
}