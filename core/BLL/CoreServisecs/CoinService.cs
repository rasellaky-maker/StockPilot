using DAL;
using BO.Models;

namespace BLL.CoreServisecs
{
    public class CoinService
    {
        private readonly CoinRepository _repo;

        public CoinService(string connectionString)
        {
            _repo = new CoinRepository(connectionString);
        }

        public async Task<List<Coin>> GetAsync()
        {
            return await _repo.SelectAsync();
        }

        public async Task<Coin?> GetAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            return await _repo.SelectByIdAsync(id);
        }

        public async Task<Coin?> GetAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return await _repo.SelectByNameAsync(name);
        }

        public async Task<List<Coin>> SearchAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            return await _repo.SearchAsync(name);
        }

        public async Task<Coin> CreateAsync(string name, string value, string? image, int userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (!decimal.TryParse(value, out decimal amount))
                throw new ArgumentException("Value must be a valid decimal number.", nameof(value));
            if (amount <= 0)
                throw new ArgumentException("Value must be greater than 0.", nameof(value));
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var result = await _repo.InsertAsync(new Coin(0, name, amount, 0, image, userId));
            return result;
        }

        public async Task<int> UpdateValueAsync(int id, decimal value)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (value <= 0)
                throw new ArgumentException("Value must be greater than 0.", nameof(value));

            return await _repo.UpdateValueAsync(id, value);
        }

        public async Task<int> UpdateAsync(int id, string name, string value, string numberofbuyers, int userId)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var coin = await GetAsync(id);
            if (coin == null)
                throw new InvalidOperationException($"Coin with id {id} not found.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (!decimal.TryParse(value, out decimal amount))
                throw new ArgumentException("Value must be a valid decimal number.", nameof(value));
            if (amount <= 0)
                throw new ArgumentException("Value must be greater than 0.", nameof(value));
            if (!int.TryParse(numberofbuyers, out int number))
                throw new ArgumentOutOfRangeException(nameof(numberofbuyers));
            // Fix: 0 Käufer ist erlaubt (eine neue Coin hat noch keine), nur negative Werte sind ungültig.
            if (number < 0)
                throw new ArgumentException("Number of buyers cannot be negative.", nameof(numberofbuyers));

            // Fix: das vorhandene Bild (coin.Image) mitgeben, sonst wird es beim Update überschrieben.
            var result = await _repo.UpdateAsync(id, new Coin(id, name, amount, number, coin.Image, userId));
            return result;
        }

        public async Task<int> RandomizeValuesAsync(decimal maxPercent)
        {
            if (maxPercent <= 0 || maxPercent >= 100)
                throw new ArgumentOutOfRangeException(nameof(maxPercent));

            return await _repo.RandomizeValuesAsync(maxPercent);
        }
    }
}