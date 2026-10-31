using BO.Models;
using DAL;

namespace BLL.CoreServisecs
{
    public class UserCoinService
    {
        private const decimal OwnerShareRate = 0.10m;

        private readonly CoinRepository _coinRepository;
        private readonly WalletRepository _walletRepository;
        private readonly UsersCoinsRepository _usersCoinsRepository;

        public UserCoinService(string connectionString)
        {
            _coinRepository = new CoinRepository(connectionString);
            _walletRepository = new WalletRepository(connectionString);
            _usersCoinsRepository = new UsersCoinsRepository(connectionString);
        }

        public async Task<bool> BuyAsync(int userId, int coinId, decimal quantity = 1)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (coinId <= 0)
                throw new ArgumentOutOfRangeException(nameof(coinId));
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            var coin = await _coinRepository.SelectByIdAsync(coinId);
            if (coin is null)
                return false;

            int ownerId = coin.UserId ?? 0;

            if (ownerId == userId)
                throw new OwnCoinPurchaseException();

            var wallet = await _walletRepository.SelectByUserIdAsync(userId);
            if (wallet is null)
                return false;

            decimal cost = coin.Value * quantity;
            if (wallet.Balance < cost)
                return false;

            decimal ownerShare = ownerId > 0
                ? Math.Round(cost * OwnerShareRate, 8)
                : 0m;

            return await _usersCoinsRepository.BuyAsync(userId, coinId, quantity, cost, ownerId, ownerShare);
        }

        /// <returns>Der Erlös, oder null wenn der Verkauf nicht möglich war.</returns>
        public async Task<decimal?> SellAsync(int userId, int coinId, decimal quantity = 1)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));
            if (coinId <= 0)
                throw new ArgumentOutOfRangeException(nameof(coinId));
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            return await _usersCoinsRepository.SellAsync(userId, coinId, quantity);
        }

        public async Task<List<UsersCoins>> ShowCoins(int userId)
        {
            var list = await _usersCoinsRepository.SelectByUserIdAsync(userId);
            return list;
        }
    }
}