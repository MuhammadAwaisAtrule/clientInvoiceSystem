using Client_Invoice_System.Components;
using Client_Invoice_System.Data;
using Client_Invoice_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Client_Invoice_System.Repositories
{
    public class OwnerPaymentProfileRepository : GenericRepository<PaymentProfile>
    {
        public OwnerPaymentProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PaymentProfile> GetOwnerPaymentProfileAsync()
        {
            return await _dbSet.FirstOrDefaultAsync() ?? new PaymentProfile();
        }

        public async Task UpdateOwnerPaymentProfileAsync(PaymentProfile profile)
        {
            var existingProfile = await _dbSet.FirstOrDefaultAsync();
            if (existingProfile == null)
            {
                await _dbSet.AddAsync(profile);
            }
            else
            {
                existingProfile.IBANNumber = profile.IBANNumber;
                existingProfile.Currency = profile.Currency;
                existingProfile.AccountTitle = profile.AccountTitle;
                existingProfile.AccountNumber = profile.AccountNumber;
            }
            await _context.SaveChangesAsync();
        }
    }
}
