namespace BO.Models
{
    public class CoinPriceHistory
    {
        public int Id { get; set; }
        public Coin? Coin { get; set; }
        public decimal Price { get; set; }
        public DateTime RecordedAt { get; set; }

        public CoinPriceHistory(int id, Coin coin, decimal price, DateTime recordedAt)
        {
            Id = id;
            Coin = coin;
            Price = price;
            RecordedAt = recordedAt;
        }
        public CoinPriceHistory() 
        { }

        public override string ToString()
        {
            return $"{Coin.Name} - {Price} EUR ({RecordedAt.ToString("dddd.MM.yyyy")})";
        }
    }
}
