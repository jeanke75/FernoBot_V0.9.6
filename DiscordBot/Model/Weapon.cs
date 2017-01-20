using System;

namespace DiscordBot.Model
{
    public class Weapon : Item
    {
        public int attackMinimum;
        public int attackMaximum;
        public int critical;
        public int strength;
        public int dexterity;
        public int stamina;
        public int sense;

        public Weapon(long id, string name, string type, int level, int valueBuy, int valueSell, int attackMinimum, int attackMaximum, int critical, int strength, int dexterity, int stamina, int sense) : base(id, name, type, level, valueBuy, valueSell)
        {
            this.attackMinimum = attackMinimum;
            this.attackMaximum = attackMaximum;
            this.critical = critical;
            this.strength = strength;
            this.dexterity = dexterity;
            this.stamina = stamina;
            this.sense = sense;
        }

        public override string ToString()
        {
            string s = "```diff" + Environment.NewLine +
                      $"+------ ITEM INFO ------+" + Environment.NewLine +
                      $"| Item ID: {id}" + Environment.NewLine +
                      $"| Name: {name}" + Environment.NewLine +
                      $"| Type: Weapon" + Environment.NewLine +
                      $"| Req. Level {level}" + Environment.NewLine +
                      $"| Attack: {attackMinimum}~{attackMaximum}" + Environment.NewLine;

            if (strength > 0 || dexterity > 0) s += $"| Strength: +{strength}  Dexterity: +{dexterity}" + Environment.NewLine;
            if (stamina > 0 || sense > 0) s += $"| Stamina: +{stamina}  Sense: +{sense}" + Environment.NewLine;
            if (valueBuy > 0) s += $"| Cost: {valueBuy} gold" + Environment.NewLine;

            s += $"| Sells for: {valueSell} gold" + Environment.NewLine +
                  "+-----------------------+```";

            return s;
        }
    }
}
