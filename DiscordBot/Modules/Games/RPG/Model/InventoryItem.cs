namespace DiscordBot.Modules.Games.RPG.Model
{
    class InventoryItem : Item
    {
        public long amount;

        public InventoryItem(int id, string name, char type, int amount) : base(id, name, type, 0, 0, 0)
        {
            this.amount = amount;
        }
    }
}
