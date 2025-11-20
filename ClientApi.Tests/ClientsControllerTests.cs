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

        [Fact]
        public async Task GetClient_ReturnsOk_WhenClientExists()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var existingClient = new Client { FirstName = "Test", LastName = "Id", CorporateName = "T", CUIT = "20-11122233-4", Email = "id@test.com", CellPhone = "1111111111", Birthdate = DateTime.Now };
            dbContext.Clients.Add(existingClient);
            await dbContext.SaveChangesAsync();

            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            // Act
            var result = await controller.GetClient(existingClient.ClientId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClient = Assert.IsType<Client>(actionResult.Value);
            Assert.Equal(existingClient.ClientId, returnedClient.ClientId);
        }

        [Fact]
        public async Task GetClient_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            // Act
            var result = await controller.GetClient(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateClient_ReturnsConflict_WhenCuitExists()
        {
            // Arrange
            var dbContext = GetDatabaseContext();

            dbContext.Clients.Add(new Client { FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-12345678-9", Email = "a@test.com", CellPhone = "1111111111", Birthdate = DateTime.Now });
            await dbContext.SaveChangesAsync();

            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            var dtoConCuitDuplicado = new CreateClientDto
            {
                FirstName = "X",
                LastName = "Y",
                CorporateName = "Z",
                CUIT = "20-12345678-9",
                Email = "otro@test.com",
                CellPhone = "2222222222",
                Birthdate = DateTime.Now
            };

            // Act
            var result = await controller.CreateClient(dtoConCuitDuplicado);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("20-12345678-9", conflictResult.Value?.ToString());
        }

        [Fact]
        public async Task UpdateClient_ReturnsOk_WhenUpdateIsValid()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var originalClient = new Client { FirstName = "Viejo", LastName = "Nombre", CorporateName = "Old Corp", CUIT = "20-11111111-1", Email = "old@test.com", CellPhone = "1111111111", Birthdate = DateTime.Now };
            dbContext.Clients.Add(originalClient);
            await dbContext.SaveChangesAsync();

            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            originalClient.FirstName = "Nuevo Nombre";
            originalClient.CorporateName = "New Corp";

            // Act
            var result = await controller.UpdateClient(originalClient.ClientId, originalClient);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedClient = Assert.IsType<Client>(actionResult.Value);
            Assert.Equal("Nuevo Nombre", updatedClient.FirstName);

            var dbClient = await dbContext.Clients.FindAsync(originalClient.ClientId);
            
            Assert.NotNull(dbClient);
            Assert.Equal("New Corp", dbClient.CorporateName);
        }

        [Fact]
        public async Task UpdateClient_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            var clientData = new Client { ClientId = 1, FirstName = "Test", LastName = "Test", CorporateName = "T", CUIT = "20-11111111-1", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            // Act
            var result = await controller.UpdateClient(50, clientData);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            var clientData = new Client { ClientId = 99, FirstName = "Test", LastName = "Test", CorporateName = "T", CUIT = "20-11111111-1", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            // Act
            var result = await controller.UpdateClient(99, clientData);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new ClientsController(dbContext, new Mock<ILogger<ClientsController>>().Object);

            // Act
            var result = await controller.DeleteClient(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}