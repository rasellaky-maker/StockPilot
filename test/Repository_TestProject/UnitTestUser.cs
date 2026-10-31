using DAL;

namespace Repository_TestProject
{
    public class UnitTestUser
    {
        private const string conn = "Server=localhost;Database=stockexchange;Uid=root;Pwd=123schule123;";

        [Fact]
        public async Task Test1()
        {
            // Arrange
            UserRepository repository = new UserRepository(conn);

            // Act
            var result = await repository.SelectAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Test2()
        {
            // Arrange
            var repository = new UserRepository(conn);
            string name = "TestUser_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var user = new BO.Models.User(0, name, "test", "test", "test_" + name, 1);

            // Act
            var result = await repository.InsertAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Username);
            Assert.NotEqual(0, result.Id);
        }

        [Fact]
        public async Task Test3()
        {
            // Arrange
            var repository = new UserRepository(conn);

            // Act
            var result = await repository.SelectByIdAsync(1);

            // Assert
            Assert.NotNull(result);
        }


        [Fact]
        public async Task Test4()
        {
            // Arrange
            var repository = new UserRepository(conn);
            string newName = "UpdateTestUser_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var updatedCoin = new BO.Models.User(0, newName, "test", "test", newName + "@gmail.com", 0);

            // Act
            var result = await repository.UpdateAsync(1, updatedCoin);

            // Assert
            Assert.NotEqual(0, result);

            // Verify persistence
            var verifyResult = await repository.SelectByIdAsync(result);
            Assert.NotNull(verifyResult);
        }
    }
}
