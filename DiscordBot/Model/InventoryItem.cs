namespace DiscordBot.Model
{
    class InventoryItem : Item
    {
        public long amount;

        public InventoryItem(long id, string name, string type, long amount) : base(id, name, type, 0, 0, 0)
        {
            this.amount = amount;
        }
    }
}
