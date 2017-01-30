namespace DiscordBot.Model
{
    public class Drop : Item
    {
        public int amount;

        public Drop(int id, string name, int amount) : base(id, name, 'I', 0, 0, 0)
        {
            this.amount = amount;
        }
    }
}
