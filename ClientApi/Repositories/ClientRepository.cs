using ClientApi.Data;
using ClientApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _context.Clients.ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Client client)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Client>> SearchByNameAsync(string name)
        {
            return await _context.Clients
                .FromSqlRaw("SELECT * FROM search_clients(@p0)", name)
                .ToListAsync();
        }

        public async Task<Client?> GetConflictAsync(string cuit, string email)
        {
            return await _context.Clients
                .Where(c => c.CUIT == cuit || c.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EmailExistsForOtherClientAsync(string email, int excludeClientId)
        {
            return await _context.Clients
                .AnyAsync(c => c.Email == email && c.ClientId != excludeClientId);
        }
    }
}