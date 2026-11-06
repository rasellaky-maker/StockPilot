namespace BO.Models
{
    public class UsersCoins
    {
        public int Id {  get; set; }
        public User User {  get; set; }
        public Coin Coin { get; set; }
        public decimal Quantity { get; set; }
        public DateTime AcquisitionDate { get; set; }

        public UsersCoins() { }

        public UsersCoins(int id, User user, Coin coin, decimal quantity, DateTime acquisitionDate)
        {
            Id = id;
            User = user;
            Coin = coin;
            Quantity = quantity;
            AcquisitionDate = acquisitionDate;
        }
        public override string ToString()
        {
            return Coin.ToString();
        }
    }
}
