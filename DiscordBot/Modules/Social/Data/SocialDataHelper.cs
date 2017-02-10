using System.Collections.Generic;

namespace DiscordBot.Modules.Social.Data
{
    public class SocialDataHelper
    {
        public List<string> getKillOptions() {
            List<string> l = new List<string>();
            l.Add("{0} tried to kill {1} but failed.");
            l.Add("{0} crushed {1} with a sledgehammer.");
            l.Add("{0} stabbed {1} with a rusty spoon.");
            l.Add("{0} threw {1} under a train.");
            l.Add("{0} ran {1} over using a stolen tractor.");
            l.Add("{0} blew {1} up with a bomb. One could say that it went out with a BANG.");
            l.Add("{0} killed {1} by dancing. Don't blame it on the sunshine, don't blame it on the moonlight, don't blame it on the good times, blame it on the boogie.");
            l.Add("{0} killed {1} but nobody knows how...");
            l.Add("{0} shot {1} with a Desert Eagle.");
            l.Add("{0} hang {1} under a bridge with by a rope tied around his neck.");
            l.Add("{0} suffocated {1} in a pile of very smelly socks, eww.");
            l.Add("{0} dropped a big anvil on {1}.");
            l.Add("{0} killed {1} by putting bleach in his milk.");
            return l;
        }
    }
}
