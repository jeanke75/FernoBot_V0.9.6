using System;

namespace DiscordBot.Model
{
    public class Item
    {
        public long id;
        public string name;
        public string type;
        public int valueBuy;
        public int valueSell;
        public int level;

        internal Item(long id, string name, string type, int level, int valueBuy, int valueSell)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.valueBuy = valueBuy;
            this.valueSell = valueSell;
            this.level = level;
        }

        public override string ToString()
        {
            string s = "```diff" + Environment.NewLine +
                      $"+------ ITEM INFO ------+" + Environment.NewLine +
                      $"| Item ID: {id}" + Environment.NewLine +
                      $"| Name: {name}" + Environment.NewLine +
                      $"| Cost: {valueBuy} gold" + Environment.NewLine +
                      $"| Sells for: {valueSell} gold" + Environment.NewLine;

            if (level > 0) s += $"| Req. Level: {level}" + Environment.NewLine;

            s += "+-----------------------+```";
            
            return s;
        }
    }
}
