using System;

namespace DiscordBot.Model
{
    public class Armor : Item
    {
        public string subtype;
        public int defense;
        public int strength;
        public int dexterity;
        public int stamina;
        public int sense;

        public Armor(long id, string name, string type, string subtype, int level, int valueBuy, int valueSell, int defense, int strength, int dexterity, int stamina, int sense) : base(id, name, type, level, valueBuy, valueSell)
        {
            this.subtype = subtype;
            this.defense = defense;
            this.strength = strength;
            this.dexterity = dexterity;
            this.stamina = stamina;
            this.sense = sense;
        }

        public override string ToString()
        {
            string armorType = "";
            switch(subtype)
            {
                case "H":
                    armorType = "Helmet";
                    break;
                case "U":
                    armorType = "Upper Armor";
                    break;
                case "P":
                    armorType = "Pants";
                    break;
                case "B":
                    armorType = "Boots";
                    break;
                case "G":
                    armorType = "Gauntlets";
                    break;
                case "M":
                    armorType = "Mantle";
                    break;
                case "S":
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
            if (stamina > 0 || sense > 0) s += $"| Stamina: +{stamina}  Sense: +{sense}" + Environment.NewLine;
            if (valueBuy > 0) s += $"| Cost: {valueBuy} gold" + Environment.NewLine;

            s += $"| Sells for: {valueSell} gold" + Environment.NewLine +
                  "+-----------------------+```";

            return s;
        }
    }
}
