using System;
using Discord;
using DiscordBot.Helpers;

namespace DiscordBot.Model
{
    public class Stats
    {
        public string name;
        public short health;
        public byte level;
        public int experience;
        public long gold;
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

        private byte _baseCritical
        {
            get
            {
                return 10;
            }
        }
        #endregion
        #region user assigned stats
        private short _strength;
        private short _dexterity;
        private short _stamina;
        private short _luck;
        #endregion

        public short strength
        {
            get
            {
                return (short)((int)Math.Floor(_baseStrength) + _strength + equipment.getEquipmentStrengthBonus());
            }
        }

        public short dexterity
        {
            get
            {
                return (short)((int)Math.Floor(_baseDexterity) + _dexterity + equipment.getEquipmentDexterityBonus());
            }
        }

        public short stamina
        {
            get
            {
                return (short)((int)Math.Floor(_baseStamina) + _stamina + equipment.getEquipmentStaminaBonus());
            }
        }

        public byte luck
        {
            get
            {
                int tmp = (byte)Math.Floor(_baseLuck) + _luck + equipment.getEquipmentLuckBonus();
                return (byte)(tmp <= 100 ? tmp : 100);
            }
        }

        public short statpoints
        {
            get
            {
                return (short)(((level - 1) * 3 + (int)Math.Floor(level * 0.2) * 2) - (_strength + _dexterity + _stamina + _luck));
            }
        }

        public short healthMax
        {
            get
            {
                return (short)((int)Math.Floor(72.6 + (level - 1) * 7.3 + Math.Floor(level * 0.2) * 0.1 + (_stamina + equipment.getEquipmentStaminaBonus()) * 2.2));
            }
        }

        public short attackMin
        {
            get
            {
                return (short)((int)Math.Floor(_baseStrength + _baseDexterity * 0.2 + (_strength + equipment.getEquipmentStrengthBonus()) * 0.5 + (_dexterity + equipment.getEquipmentDexterityBonus()) * 0.3) + equipment.getEquipmentAttackMinBonus());
            }
        }

        public short attackMax
        {
            get
            {
                return (short)((int)Math.Floor(_baseStrength + _baseDexterity * 0.2 + (_strength + equipment.getEquipmentStrengthBonus()) * 0.5 + (_dexterity + equipment.getEquipmentDexterityBonus()) * 0.3) + equipment.getEquipmentAttackMaxBonus());
            }
        }

        public short defense
        {
            get
            {
                return (short)((int)Math.Floor((_baseDexterity + _dexterity + equipment.getEquipmentDexterityBonus()) * 0.6) + equipment.getEquipmentDefenseBonus());
            }
        }

        public byte critical
        {
            get
            {
                return (byte)(_baseCritical + equipment.getEquipmentCriticalBonus());
            }
        }

        public int experienceNeeded
        {
            get
            {
                return (int)Math.Round(50 * Math.Pow(1.1, level));
            }
        }

        public Stats(User user, byte level, short str, short dex, short sta, short lck, short health, int experience, long gold, Equipment equipment) {
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
