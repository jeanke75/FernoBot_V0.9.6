using System;
using System.Collections.Generic;

namespace DiscordBot.Model
{
    class Monster
    {
        internal long id;
        internal string name;
        private int _health;
        internal int health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = (value <= healthMax ? value : healthMax);
            }
        }
        internal int healthMax;
        internal int healthRegen;
        internal int defenseMinimum;
        internal int defenseNoCritical;
        internal int damageMinimum;
        internal int experience;
        internal int goldMinimum;
        internal int goldMaximum;
        List<Item> items;

        internal Monster(long id, string name, int health, int healthMax, int healthRegen, int defenseMinimum, int defenseNoCritical, int damageMinimum)
        {
            this.id = id;
            this.name = name;
            this.healthMax = healthMax;
            this.health = health;
            this.healthRegen = healthRegen;
            this.defenseMinimum = defenseMinimum;
            this.defenseNoCritical = defenseNoCritical;
            this.damageMinimum = damageMinimum;
            experience = -1;
            goldMinimum = -1;
            goldMaximum = -1;
            items = new List<Item>();
        }

        internal Monster(long id, string name, int health, int healthMax, int healthRegen, int defenseMinimum, int defenseNoCritical, int damageMinimum, int experience, int goldMinimum, int goldMaximum)
        {
            this.id = id;
            this.name = name;
            this.healthMax = healthMax;
            this.health = health;
            this.healthRegen = healthRegen;
            this.defenseMinimum = defenseMinimum;
            this.defenseNoCritical = defenseNoCritical;
            this.damageMinimum = damageMinimum;
            this.experience = experience;
            this.goldMinimum = goldMinimum;
            this.goldMaximum = goldMaximum;
            items = new List<Item>();
        }

        internal Monster(long id, string name, int health, int healthMax, int healthRegen, int defenseMinimum, int defenseNoCritical, int damageMinimum, int experience, int goldMinimum, int goldMaximum, List<Item> items)
        {
            this.id = id;
            this.name = name;
            this.healthMax = healthMax;
            this.health = health;
            this.healthRegen = healthRegen;
            this.defenseMinimum = defenseMinimum;
            this.defenseNoCritical = defenseNoCritical;
            this.damageMinimum = damageMinimum;
            this.experience = experience;
            this.goldMinimum = goldMinimum;
            this.goldMaximum = goldMaximum;
            this.items = items;
        }

        internal void setDrops(List<Item> items)
        {
            this.items = items;
        }

        public override string ToString()
        {
            string s = "```diff" + Environment.NewLine +
                      $"+------ MONSTER INFO ------+" + Environment.NewLine +
                      $"| Monster ID: {id}" + Environment.NewLine +
                      $"| Name: {name}" + Environment.NewLine +
                      $"| Health: {healthMax}" + Environment.NewLine +
                      $"| Health regen: {healthRegen}" + Environment.NewLine +
                      $"| Minimum damage: {damageMinimum}" + Environment.NewLine +
                      $"| Minimum defense: {defenseMinimum}" + Environment.NewLine +
                      $"| No crit. defense: {defenseNoCritical}" + Environment.NewLine +
                      $"| Experience: {experience}" + Environment.NewLine +
                      $"| Gold drop: {goldMinimum}~{goldMaximum}" + Environment.NewLine +
                      $"| Possible item drops:" + Environment.NewLine;
            
            foreach(Item item in items) s += $"|   {item.name}" + Environment.NewLine;

            return s + "+--------------------------+```";
        }
    }
}
