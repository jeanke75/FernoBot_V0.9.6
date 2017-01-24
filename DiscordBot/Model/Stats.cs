using System;
using Discord;
using DiscordBot.Helpers;

namespace DiscordBot.Model
{
    public class Stats
    {
        public string name;
        public int level;
        public int experience;
        public int health;
        public int gold;
        public Equipment equipment;

        #region base stats
        private double _baseStrength
        {
            get
            {
                return 2 + (level - 1) * 0.4;
            }
        }

        private double _baseDexterity
        {
            get
            {
                return 5 + (level - 1);
            }
        }

        private double _baseStamina
        {
            get
            {
                return 3 + (level - 1) * 0.6;
            }
        }

        private double _baseLuck
        {
            get
            {
                return 4 + (level - 1) * 0.8;
            }
        }

        private int _baseCritical
        {
            get
            {
                return 10;
            }
        }
        #endregion
        #region user assigned stats
        private int _strength;
        private int _dexterity;
        private int _stamina;
        private int _luck;
        #endregion

        public int strength
        {
            get
            {
                return (int)Math.Floor(_baseStrength) + _strength + equipment.getEquipmentStrengthBonus(); ;
            }
        }

        public int dexterity
        {
            get
            {
                return (int)Math.Floor(_baseDexterity) + _dexterity + equipment.getEquipmentDexterityBonus();
            }
        }

        public int stamina
        {
            get
            {
                return (int)Math.Floor(_baseStamina) + _stamina + equipment.getEquipmentStaminaBonus();
            }
        }

        public int luck
        {
            get
            {
                return (int)Math.Floor(_baseLuck) + _luck + equipment.getEquipmentSenseBonus();
            }
        }

        public int statpoints
        {
            get
            {
                return ((level - 1) * 3 + (int)Math.Floor(level * 0.2) * 2) - (_strength + _dexterity + _stamina + _luck);
            }
        }

        public int healthMax
        {
            get
            {
                return (int)Math.Floor(72.6 + (level - 1) * 7.3 + Math.Floor(level * 0.2) * 0.1 + (_stamina + equipment.getEquipmentStaminaBonus()) * 2.2);
            }
        }

        public int attackMin
        {
            get
            {
                return (int)Math.Floor(_baseStrength + _baseDexterity * 0.2 + (_strength + equipment.getEquipmentStrengthBonus()) * 0.5 + (_dexterity + equipment.getEquipmentDexterityBonus()) * 0.3) + equipment.getEquipmentAttackMinBonus();
            }
        }

        public int attackMax
        {
            get
            {
                return (int)Math.Floor(_baseStrength + _baseDexterity * 0.2 + (_strength + equipment.getEquipmentStrengthBonus()) * 0.5 + (_dexterity + equipment.getEquipmentDexterityBonus()) * 0.3) + equipment.getEquipmentAttackMaxBonus();
            }
        }

        public int defense
        {
            get
            {
                return (int)Math.Floor((_baseDexterity + _dexterity + equipment.getEquipmentDexterityBonus()) * 0.6) + equipment.getEquipmentDefenseBonus();
            }
        }

        public int critical
        {
            get
            {
                return _baseCritical + equipment.getEquipmentCriticalBonus();
            }
        }

        public int experienceNeeded
        {
            get
            {
                return (int)Math.Round(50 * Math.Pow(1.1, level));
            }
        }

        public Stats(User user, int level, int str, int dex, int sta, int lck, int health, int experience, int gold, Equipment equipment) {
            name = Helper.getDiscordDisplayName(user);
            this.level = level;
            _strength = str;
            _dexterity = dex;
            _stamina = sta;
            _luck = lck;
            this.health = health;
            this.experience = experience;
            this.gold = gold;
            this.equipment = equipment;
        }

        public override string ToString() {
            string s = "```diff" + Environment.NewLine +
                      $"+------ {name}'s stats ------+" + Environment.NewLine +
                      $"| Level {level} ({experience}/{experienceNeeded})" + Environment.NewLine +
                      $"| Health: {health}/{healthMax}" + Environment.NewLine +
                      $"| Attack: {attackMin}~{attackMax}" + Environment.NewLine +
                      $"| Defense: {defense}" + Environment.NewLine +
                      $"| Critical: {critical}" + Environment.NewLine +
                       "|" + Environment.NewLine +
                      $"| Attributes: {statpoints}" + Environment.NewLine +
                      $"| Strength: {strength}  Dexterity: {dexterity}" + Environment.NewLine +
                      $"| Stamina: {stamina}  Sense: {luck}" + Environment.NewLine +
                       "|" + Environment.NewLine +
                      $"| Weapon: {(equipment.weapon != null ? equipment.weapon.name : "/")}  Shield: {(equipment.shield != null ? equipment.shield.name : "/")}" + Environment.NewLine +
                      $"| Helmet: {(equipment.helmet != null ? equipment.helmet.name : "/")}  Mantle: {(equipment.mantle != null ? equipment.mantle.name : "/")}" + Environment.NewLine +
                      $"| Upper: {(equipment.upper != null ? equipment.upper.name : "/")}  Gaunlets: {(equipment.gauntlets != null ? equipment.gauntlets.name : "/")}" + Environment.NewLine +
                      $"| Pants: {(equipment.pants != null ? equipment.pants.name : "/")}  Boots: {(equipment.boots != null ? equipment.boots.name : "/")}" + Environment.NewLine +
                       "|" + Environment.NewLine +
                      $"| Gold: {gold}" + Environment.NewLine +
                       "+-------";

            for (int i = 0; i < name.Length; i++) s += "-";

            return s + "---------------+```";
        }
    }
}
