namespace BO.Models
{
    public class Coin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public int NumberOfBuyers { get; set; }
        public string? Image { get; set; }
        public int? UserId { get; set; }

        public Coin() { }
        public Coin(int id, string name, decimal value, int numberOfBuyers, int? userId)
        {
            Id = id;
            Name = name;
            Value = value;
            NumberOfBuyers = numberOfBuyers;
            UserId = userId;
        }
        public Coin(int id, string name, decimal value, int numberOfBuyers, string? imag, int? userId):this(id, name, value, numberOfBuyers, userId)
        { 
            Image = imag;
        }

        public override string ToString()
        {
            return $"{Name} - {Value} EUR (Buyed by {NumberOfBuyers} persons)";
        }
    }
}
