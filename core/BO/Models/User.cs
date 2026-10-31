using System.Text.Json.Serialization;

namespace BO.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        [JsonIgnore] public string PassHash {  get; set; }
        [JsonIgnore] public string PassSalt { get; set; }
        public string Email { get; set; }
        public byte AvailableCoins { get; set; }

        public User() { }
        public User(int id, string username, string passHash, string passSalt, string email, byte availableCoins)
        {
            Id = id;
            Username = username;
            PassHash = passHash;
            PassSalt = passSalt;
            Email = email;
            AvailableCoins = availableCoins;
        }

        public override string ToString()
        {
            return "(" + Id + ")" + Username;
        }

        public string AllInfo()
        {
            return $"ID: {Id}\nUsername: {Username}\nEmail: {Email}\n Coins to create: {AvailableCoins}";
        }
    }
}
