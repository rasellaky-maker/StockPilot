using DAL;
using BO;
using MySqlConnector;

namespace Repository_TestProject
{
    public class UnitTestCoin
    {
        private const string conn = "Server=localhost;Database=stockexchange;Uid=root;Pwd=123schule123;";
        [Fact]
        public async Task Test1()
        {
            // Arrange
            CoinRepository repository = new CoinRepository(conn);

            // Act
            var result = await repository.SelectAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Test2()
        {
            // Arrange
            var repository = new CoinRepository(conn);
            string name = "TestCoin_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var coin = new BO.Models.Coin(1, name, 22.342m, 0);

            // Act
            var result = await repository.InsertAsync(coin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }

        [Fact]
        public async Task Test3()
        {
            // Arrange
            var repository = new CoinRepository(conn);

            // Act
            var result = await repository.SelectByIdAsync(1);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Test4()
        {
            // Arrange
            var repository = new CoinRepository(conn);
            decimal newPrice = 99.99m;
            string newName = "UpdatedTestCoin_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var updatedCoin = new BO.Models.Coin(0, newName, newPrice, 0);

            // Act
            var result = await repository.UpdateAsync(1, updatedCoin);

            // Assert
            Assert.NotEqual(0, result);
            
            // Verify persistence
            var verifyResult = await repository.SelectByIdAsync(result);
            Assert.NotNull(verifyResult);
            Assert.Equal(newName, verifyResult.Name);
        }

        [Fact]
        public async Task Test5()
        {
            // Arrange
            var repository = new CoinRepository(conn);

            // Act
            var result = await repository.SearchAsync("Bit");


            // Assert
            Assert.NotEqual(0, result.Count);
        }
    }
}