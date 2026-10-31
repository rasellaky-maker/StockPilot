using BO.Models;
using DAL;
using System.Dynamic;
using System.Threading.Tasks;

namespace BLL.CoreServisecs
{
    public class WalletService
    {
        private WalletRepository _repo = null;
        public WalletService(string connectionString)
        {
            _repo = new WalletRepository(connectionString);
        }

        public async Task<List<Wallet>> GetAsync()
        {
            return await _repo.SelectAsync();
        }

        public async Task<Wallet?> GetAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException(nameof(id));

            return await _repo.SelectByIdAsync(id);
        }

        public async Task<Wallet?> GetByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentNullException(nameof(userId));

            var result = await _repo.SelectByUserIdAsync(userId);
            return result;
        }

        public async Task<Wallet> CreateAsync(Wallet wallet)
        {
            //               10.000
            wallet.Balance = 10000;
            return await _repo.InsertAsync(wallet);
        }

        public async Task<int> UpdateAsync(int id, Wallet wallet)
        {
            return await _repo.UpdateAsync(id, wallet);
        }
    }
}
