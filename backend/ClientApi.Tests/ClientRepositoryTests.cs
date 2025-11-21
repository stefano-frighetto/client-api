using ClientApi.Data;
using ClientApi.Models;
using ClientApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;

namespace ClientApi.Tests
{
    public class ClientRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddClient()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var context = GetDbContext(dbName);
            var repo = new ClientRepository(context);
            var client = new Client { FirstName = "Test", LastName = "User", CorporateName = "Corp", CUIT = "20-12345678-9", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            // Act
            await repo.AddAsync(client);

            // Assert
            var savedClient = await context.Clients.FirstOrDefaultAsync();
            Assert.NotNull(savedClient);
            Assert.Equal("Test", savedClient.FirstName);
        }

        [Fact]
        public async Task EmailExistsForOtherClientAsync_ReturnsTrue_WhenConflictExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(new Client { ClientId = 1, FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-111", Email = "dup@test.com", CellPhone = "111", Birthdate = DateTime.Now });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);

                // Act
                var result = await repo.EmailExistsForOtherClientAsync("dup@test.com", 2);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public async Task GetConflictAsync_ReturnsClient_WhenCuitExists()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(new Client { ClientId = 1, FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-DUPLICADO-9", Email = "unique@test.com", CellPhone = "111", Birthdate = DateTime.Now });
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var conflict = await repo.GetConflictAsync("20-DUPLICADO-9", "other@test.com");
                Assert.NotNull(conflict);
            }
        }
    }
}