namespace BO.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public User User { get; set; } = new();
        public decimal Balance { get; set; }
        public List<UsersCoins> UsersCoins { get; set; } = new();

        public Wallet() { }

        public Wallet(int id, User user, decimal balance, List<UsersCoins> usersCoins)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (usersCoins == null)
                throw new ArgumentNullException(nameof(usersCoins));

            Id = id;
            User = user;
            Balance = balance;
            UsersCoins = usersCoins;
        }

        public Wallet(int id, User user, decimal balance):this(id, user, balance, new List<UsersCoins>())
        { }

        public override string ToString()
        {
            return $"Wallet of {User.Username}";
        }
    }
}
