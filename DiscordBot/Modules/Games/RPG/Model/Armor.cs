using System;

namespace DiscordBot.Modules.Games.RPG.Model
{
    public class Armor : Item
    {
        public char subtype;
        public short defense;
        public byte strength;
        public byte dexterity;
        public byte stamina;
        public byte luck;

        public Armor(int id, string name, char type, char subtype, byte level, int valueBuy, int valueSell, short defense, byte strength, byte dexterity, byte stamina, byte luck) : base(id, name, type, level, valueBuy, valueSell)
        {
            this.subtype = subtype;
            this.defense = defense;
            this.strength = strength;
            this.dexterity = dexterity;
            this.stamina = stamina;
            this.luck = luck;
        }

        public override string ToString()
        {
            string armorType = "";
            switch(subtype)
            {
                case 'H':
                    armorType = "Helmet";
                    break;
                case 'U':
                    armorType = "Upper Armor";
                    break;
                case 'P':
                    armorType = "Pants";
                    break;
                case 'B':
                    armorType = "Boots";
                    break;
                case 'G':
                    armorType = "Gauntlets";
                    break;
                case 'M':
                    armorType = "Mantle";
                    break;
                case 'S':
                    armorType = "Shield";
                    break;
            }

            string s = "```diff" + Environment.NewLine +
                      $"+------ ITEM INFO ------+" + Environment.NewLine +
                      $"| Item ID: {id}" + Environment.NewLine +
                      $"| Name: {name}" + Environment.NewLine +
                      $"| Type: {armorType}" + Environment.NewLine +
                      $"| Req. Level {level}" + Environment.NewLine +
                      $"| Defense: {defense}" + Environment.NewLine;
            
            if (strength > 0 || dexterity > 0) s += $"| Strength: +{strength}  Dexterity: +{dexterity}" + Environment.NewLine;
            if (stamina > 0 || luck > 0) s += $"| Stamina: +{stamina}  Luck: +{luck}" + Environment.NewLine;
            if (valueBuy > 0) s += $"| Cost: {valueBuy} gold" + Environment.NewLine;

            s += $"| Sells for: {valueSell} gold" + Environment.NewLine +
                  "+-----------------------+```";

            return s;
        }
    }
}
