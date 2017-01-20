using System.Collections.Generic;

namespace DiscordBot.Model
{
    public class Equipment
    {
        public Weapon weapon;
        public Armor shield;
        public Armor helmet;
        public Armor mantle;
        public Armor upper;
        public Armor gauntlets;
        public Armor pants;
        public Armor boots;

        public Equipment(Weapon weapon, Armor shield, Armor helmet, Armor upper, Armor pants, Armor boots, Armor gauntlets, Armor mantle)
        {
            this.weapon = weapon;
            this.shield = shield;
            this.helmet = helmet;
            this.upper = upper;
            this.pants = pants;
            this.boots = boots;
            this.gauntlets = gauntlets;
            this.mantle = mantle;
        }

        public int getEquipmentStrengthBonus()
        {
            int bonus = 0;
            if (weapon != null) bonus += weapon.strength;
            if (helmet != null) bonus += helmet.strength;
            if (upper != null) bonus += upper.strength;
            if (pants != null) bonus += pants.strength;
            if (boots != null) bonus += boots.strength;
            if (gauntlets != null) bonus += gauntlets.strength;
            if (shield != null) bonus += shield.strength;
            if (mantle != null) bonus += mantle.strength;
            return bonus;
        }

        public int getEquipmentDexterityBonus()
        {
            int bonus = 0;
            if (weapon != null) bonus += weapon.dexterity;
            if (helmet != null) bonus += helmet.dexterity;
            if (upper != null) bonus += upper.dexterity;
            if (pants != null) bonus += pants.dexterity;
            if (boots != null) bonus += boots.dexterity;
            if (gauntlets != null) bonus += gauntlets.dexterity;
            if (shield != null) bonus += shield.dexterity;
            if (mantle != null) bonus += mantle.dexterity;
            return bonus;
        }

        public int getEquipmentStaminaBonus()
        {
            int bonus = 0;
            if (weapon != null) bonus += weapon.stamina;
            if (helmet != null) bonus += helmet.stamina;
            if (upper != null) bonus += upper.stamina;
            if (pants != null) bonus += pants.stamina;
            if (boots != null) bonus += boots.stamina;
            if (gauntlets != null) bonus += gauntlets.stamina;
            if (shield != null) bonus += shield.stamina;
            if (mantle != null) bonus += mantle.stamina;
            return bonus;
        }

        public int getEquipmentSenseBonus()
        {
            int bonus = 0;
            if (weapon != null) bonus += weapon.sense;
            if (helmet != null) bonus += helmet.sense;
            if (upper != null) bonus += upper.sense;
            if (pants != null) bonus += pants.sense;
            if (boots != null) bonus += boots.sense;
            if (gauntlets != null) bonus += gauntlets.sense;
            if (shield != null) bonus += shield.sense;
            if (mantle != null) bonus += mantle.sense;
            return bonus;
        }

        public int getEquipmentAttackMinBonus()
        {
            return (weapon != null ? weapon.attackMinimum : 0);
        }

        public int getEquipmentAttackMaxBonus()
        {
            return (weapon != null ? weapon.attackMaximum : 0);
        }

        public int getEquipmentDefenseBonus()
        {
            int bonus = 0;
            if (helmet != null) bonus += helmet.defense;
            if (upper != null) bonus += upper.defense;
            if (pants != null) bonus += pants.defense;
            if (boots != null) bonus += boots.defense;
            if (gauntlets != null) bonus += gauntlets.defense;
            if (shield != null) bonus += shield.defense;
            if (mantle != null) bonus += mantle.defense;
            return bonus;
        }

        public int getEquipmentCriticalBonus()
        {
            return (weapon != null ? weapon.critical : 0);
        }

        public bool isItemEquipped(Item item)
        {
            List<Item> items = new List<Item>();
            if (weapon != null) items.Add(weapon);
            if (helmet != null) items.Add(helmet);
            if (upper != null) items.Add(upper);
            if (pants != null) items.Add(pants);
            if (boots != null) items.Add(boots);
            if (gauntlets != null) items.Add(gauntlets);
            if (shield != null) items.Add(shield);
            if (mantle != null) items.Add(mantle);

            foreach(Item itm in items)
            {
                if (itm.id == item.id) return true;
            }
            return false;
        }
    }
}
