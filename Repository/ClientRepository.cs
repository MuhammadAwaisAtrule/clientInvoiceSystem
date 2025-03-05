using Client_Invoice_System.Data;
using Client_Invoice_System.Models;
using Client_Invoice_System.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client_Invoice_System.Repository
{
    public class ClientRepository : GenericRepository<Client>
    {
        public ClientRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Client>> GetActiveClientsAsync()
        {
            return await _context.ActiveClients
                .Where(ac => ac.Status == true)
                .Select(ac => ac.Client)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(c => c.Email == email);
        }

        public async Task<Client> GetClientWithResourcesAsync(int clientId)
        {
            return await _dbSet
                .Include(c => c.Resources)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public override async Task DeleteAsync(int clientId)
        {
            var activeClient = await _context.ActiveClients.FirstOrDefaultAsync(ac => ac.ClientId == clientId);
            if (activeClient != null)
            {
                activeClient.Status = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Client> GetByIdAsync(int clientId)
        {
            return await _dbSet.FindAsync(clientId);
        }

        public async Task AddAsync(Client client)
        {
            await _dbSet.AddAsync(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _dbSet.Update(client);
            await _context.SaveChangesAsync();
        }
    }
}
