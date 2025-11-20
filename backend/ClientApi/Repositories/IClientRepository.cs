using ClientApi.Models;

namespace ClientApi.Repositories
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(Client client);
        Task<IEnumerable<Client>> SearchByNameAsync(string name);
        Task<Client?> GetConflictAsync(string cuit, string email);
        Task<bool> EmailExistsForOtherClientAsync(string email, int excludeClientId);
    }
}