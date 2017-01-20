namespace DiscordBot.Model
{
    public class Drop : Item
    {
        public int amount;

        public Drop(long id, string name, int amount) : base(id, name, "", 0, 0, 0)
        {
            this.amount = amount;
        }
    }
}
