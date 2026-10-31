using BO.Models;
using DAL;

namespace Repository_TestProject
{
    public class UnitTestWallet
    {
        private const string conn = "Server=localhost;Database=stockexchange;Uid=root;Pwd=123schule123;";
        [Fact]
        public async Task Test1()
        {
            // Arrange
            var repository = new WalletRepository(conn);

            // Act
            var result = await repository.SelectAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Test2()
        {
            // Arrange
            var repository = new WalletRepository(conn);
            var userRepository = new UserRepository(conn);
            
            // Create a real user first
            string userName = "TestUser_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var user = new User(0, userName, "test", "test", "test_" + userName, 1);
            var createdUser = await userRepository.InsertAsync(user);
            
            var wallet = new Wallet(0, createdUser, 100.5m);

            // Act
            var result = await repository.InsertAsync(wallet);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdUser.Id, result.User.Id);
            Assert.Equal(100.5m, result.Balance);
        }

        [Fact]
        public async Task Test3()
        {
            // Arrange
            var repository = new WalletRepository(conn);

            // Act
            var result = await repository.SelectByIdAsync(1);

            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public async Task Test4()
        {
            // Arrange
            var repository = new WalletRepository(conn);
            var userRepository = new UserRepository(conn);
            
            // Get or create a user for the wallet with ID 1
            var existingWallet = await repository.SelectByIdAsync(1);
            Assert.NotNull(existingWallet);
            
            decimal newBalance = 50.75m;
            var updatedWallet = new Wallet(1, existingWallet.User, newBalance);

            // Act
            var result = await repository.UpdateAsync(1, updatedWallet);

            // Assert
            Assert.Equal(1, result); // Should return the ID of updated wallet

            // Verify persistence
            var verifyResult = await repository.SelectByIdAsync(1);
            Assert.NotNull(verifyResult);
            Assert.Equal(newBalance, verifyResult.Balance);
        }
    }
}
