using System;

namespace DiscordBot.Modules.Games.RPG.Model
{
    public class Potion : Item
    {
        public short heal;

        public Potion(int id, string name, char type, byte level, int valueBuy, int valueSell, short heal) : base(id, name, type, level, valueBuy, valueSell)
        {
            this.heal = heal;
        }

        public override string ToString()
        {
            string s = "```diff" + Environment.NewLine +
                      $"+------ ITEM INFO ------+" + Environment.NewLine +
                      $"| Item ID: {id}" + Environment.NewLine +
                      $"| Name: {name}" + Environment.NewLine +
                      $"| Type: Potion" + Environment.NewLine +
                      $"| Heal: {heal} Health" + Environment.NewLine +
                      $"| Cost: {valueBuy} gold" + Environment.NewLine +
                      $"| Sells for: {valueSell} gold" + Environment.NewLine +
                       "+-----------------------+```";

            return s;
        }
    }
}
