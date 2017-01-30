using System;

namespace DiscordBot.Model
{
    public class Weapon : Item
    {
        public short attackMinimum;
        public short attackMaximum;
        public byte critical;
        public byte strength;
        public byte dexterity;
        public byte stamina;
        public byte luck;

        public Weapon(int id, string name, char type, byte level, int valueBuy, int valueSell, short attackMinimum, short attackMaximum, byte critical, byte strength, byte dexterity, byte stamina, byte sense) : base(id, name, type, level, valueBuy, valueSell)
        {
            this.attackMinimum = attackMinimum;
            this.attackMaximum = attackMaximum;
            this.critical = critical;
            this.strength = strength;
            this.dexterity = dexterity;
            this.stamina = stamina;
            this.luck = sense;
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
            if (stamina > 0 || luck > 0) s += $"| Stamina: +{stamina}  Luck: +{luck}" + Environment.NewLine;
            if (valueBuy > 0) s += $"| Cost: {valueBuy} gold" + Environment.NewLine;

            s += $"| Sells for: {valueSell} gold" + Environment.NewLine +
                  "+-----------------------+```";

            return s;
        }
    }
}
