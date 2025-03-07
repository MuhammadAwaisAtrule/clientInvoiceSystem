using Client_Invoice_System.Data;
using Client_Invoice_System.Models;
using Client_Invoice_System.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client_Invoice_System.Repository
{
    public class ResourceRepository(ApplicationDbContext context) : GenericRepository<Resource>(context)
    {
        public async Task<IEnumerable<Resource>> GetResourcesByClientAsync(int clientId)
        {
            return await _dbSet
                .Include(r => r.Employee)
                .Where(r => r.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<int> GetTotalHoursConsumedAsync(int resourceId)
        {
            var resource = await _dbSet.FindAsync(resourceId);
            return resource?.ConsumedTotalHours ?? 0;
        }

        public async Task<decimal> CalculateClientBillingAsync(int clientId)
        {
            var resources = await _dbSet
                .Include(r => r.Employee)
                .Where(r => r.ClientId == clientId)
                .ToListAsync();

            return resources.Sum(r => r.ConsumedTotalHours * r.Employee.HourlyRate);
        }

        public async Task<IEnumerable<Resource>> GetAllResourcesWithDetailsAsync()
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Employee)
                .ToListAsync();
        }

        public async Task<Resource?> GetResourceDetailsAsync(int resourceId)
        {
            return await _dbSet
                .Include(r => r.Client)
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.ResourceId == resourceId);
        }
    }
}
