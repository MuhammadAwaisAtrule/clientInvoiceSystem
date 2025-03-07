using Client_Invoice_System.Data;
using Client_Invoice_System.Models;
using Client_Invoice_System.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Client_Invoice_System.Repository
{
    public class OwnerRepository : GenericRepository<OwnerProfile>
    {
        public OwnerRepository(ApplicationDbContext context) : base(context) { }

        public async Task<OwnerProfile> GetOwnerProfileAsync()
        {
            return await _dbSet.FirstOrDefaultAsync() ?? new OwnerProfile();
        }

        public async Task UpdateOwnerProfileAsync(OwnerProfile owner)
        {
            var existingOwner = await _dbSet.FirstOrDefaultAsync();
            if (existingOwner != null)
            {
                existingOwner.OwnerName = owner.OwnerName;
                existingOwner.BillingEmail = owner.BillingEmail;
                existingOwner.PhoneNumber = owner.PhoneNumber;
                existingOwner.BillingAddress = owner.BillingAddress;

                _context.Update(existingOwner);
            }
            else
            {
                await _dbSet.AddAsync(owner);
            }

            await _context.SaveChangesAsync();
        }
    }
}
