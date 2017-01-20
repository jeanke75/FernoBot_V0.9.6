using System.Collections.Generic;
using Discord;
using DiscordBot.Data;
using DiscordBot.Helpers;

namespace DiscordBot.Controllers
{
    public static class Social_Controller
    {
        public static string Kill(User caller, string target)
        {
            if (target.Equals("<@250938883968204800>")) return "Oh silly human, you can't kill me!";

            List<string> l = new SocialDataHelper().getKillOptions();
            int i = Helper.rng.Next(0, l.Count);

            return string.Format(l[i], Helper.getDiscordDisplayName(caller), (!target.Equals("") && !target.Equals(caller.Mention) ?  target : "himself"));
        }

        #region Dice
        public static string Dice(User caller, string param1, string param2)
        {
            if (!param1.Equals("")) {
                int p1;
                if (!int.TryParse(param1, out p1)) return string.Format("{0} your input was invalid.", Helper.getDiscordDisplayName(caller));
                
                if (!param2.Equals("")) {
                    int p2;
                    if (!int.TryParse(param2, out p2)) return string.Format("{0} your input was invalid.", Helper.getDiscordDisplayName(caller));
                    if (p1 >= p2) return string.Format("{0} the max value must be higher than the min value.", Helper.getDiscordDisplayName(caller));
                    return Dice(caller, p1, p2);
                }
                if (p1 == 1) return string.Format("{0} the max value must be higher than the min value 1.", Helper.getDiscordDisplayName(caller));
                return Dice(caller, p1);
            } else {
                return Dice(caller);
            }
        }

        static string Dice(User caller)
        {
            return Dice(caller, 1, 6);
        }

        static string Dice(User caller, int max)
        {
            
            return Dice(caller, 1, max);
        }
        static string Dice(User caller, int min, int max)
        {
            return string.Format("{0} rolled a {1}!", Helper.getDiscordDisplayName(caller), Helper.rng.Next(min, max + 1));
        }
        #endregion
    }
}
