using ClientApi.Controllers;
using ClientApi.Data;
using ClientApi.DTOs;
using ClientApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClientApi.Tests
{
    public class ClientsControllerTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact]
        public async Task GetClients_ReturnsOkResult_WithListOfClients()
        {
            // Arrange
            var dbContext = GetDatabaseContext();

            dbContext.Clients.Add(new Client { FirstName = "Juan", LastName = "Perez", CorporateName = "Juan Corp", CUIT = "20-11111111-1", Email = "juan@test.com", CellPhone = "1122334455", Birthdate = DateTime.Now });
            dbContext.Clients.Add(new Client { FirstName = "Maria", LastName = "Gomez", CorporateName = "Maria SRL", CUIT = "27-22222222-2", Email = "maria@test.com", CellPhone = "1122334466", Birthdate = DateTime.Now });
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<ClientsController>>();
            var controller = new ClientsController(dbContext, mockLogger.Object);

            // Act
            var result = await controller.GetClients();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClients = Assert.IsType<List<Client>>(actionResult.Value);
            Assert.Equal(2, returnedClients.Count);
        }

        [Fact]
        public async Task CreateClient_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var mockLogger = new Mock<ILogger<ClientsController>>();
            var controller = new ClientsController(dbContext, mockLogger.Object);

            var newClientDto = new CreateClientDto
            {
                FirstName = "Nuevo",
                LastName = "Cliente",
                CorporateName = "New SA",
                CUIT = "20-33333333-3",
                Email = "new@test.com",
                CellPhone = "1122334477",
                Birthdate = DateTime.Now
            };

            // Act
            var result = await controller.CreateClient(newClientDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdClient = Assert.IsType<Client>(createdAtActionResult.Value);
            Assert.Equal("Nuevo", createdClient.FirstName);
            Assert.NotEqual(0, createdClient.ClientId);
        }

        [Fact]
        public async Task CreateClient_ReturnsConflict_WhenEmailExists()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            dbContext.Clients.Add(new Client { FirstName = "Existente", LastName = "Uno", CorporateName = "A", CUIT = "20-99999999-9", Email = "duplicado@test.com", CellPhone = "1111111111", Birthdate = DateTime.Now });
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<ClientsController>>();
            var controller = new ClientsController(dbContext, mockLogger.Object);

            var dtoConEmailDuplicado = new CreateClientDto
            {
                FirstName = "Intruso",
                LastName = "Dos",
                CorporateName = "B",
                CUIT = "20-88888888-8",
                Email = "duplicado@test.com",
                CellPhone = "2222222222",
                Birthdate = DateTime.Now
            };

            // Act
            var result = await controller.CreateClient(dtoConEmailDuplicado);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("duplicado@test.com", conflictResult.Value?.ToString());
        }

        [Fact]
        public async Task DeleteClient_ReturnsNoContent_WhenClientExists()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var clientToDelete = new Client { FirstName = "Borrar", LastName = "Me", CorporateName = "Bye", CUIT = "20-55555555-5", Email = "bye@test.com", CellPhone = "1155555555", Birthdate = DateTime.Now };
            dbContext.Clients.Add(clientToDelete);
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<ClientsController>>();
            var controller = new ClientsController(dbContext, mockLogger.Object);

            // Act
            var result = await controller.DeleteClient(clientToDelete.ClientId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Null(await dbContext.Clients.FindAsync(clientToDelete.ClientId));
        }
    }
}