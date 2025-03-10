using Client_Invoice_System.Components;
using Client_Invoice_System.Data;
using Client_Invoice_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client_Invoice_System.Repositories
{
    public class OwnerPaymentProfileRepository : GenericRepository<PaymentProfile>
    {
        public OwnerPaymentProfileRepository(ApplicationDbContext context) : base(context) { }

        // ✅ Get all payment profiles
        public async Task<List<PaymentProfile>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all payment profiles: {ex.Message}");
                return new List<PaymentProfile>(); // Return an empty list on failure
            }
        }

        // ✅ Get a payment profile by ID
        public async Task<PaymentProfile> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching payment profile by ID {id}: {ex.Message}");
                return null;
            }
        }

        // ✅ Get the first owner payment profile or create a new one
        public async Task<PaymentProfile> GetOwnerPaymentProfileAsync()
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync() ?? new PaymentProfile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching owner payment profile: {ex.Message}");
                return new PaymentProfile(); // Return a new empty profile on failure
            }
        }

        // ✅ Add a new payment profile
        public async Task<bool> AddAsync(PaymentProfile profile)
        {
            try
            {
                var ownerExists = await _context.Owners.AnyAsync(o => o.Id == profile.OwnerId);
                if (!ownerExists)
                {
                    Console.WriteLine("Invalid OwnerId. Owner does not exist.");
                    return false;
                }

                await _dbSet.AddAsync(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding payment profile: {ex.Message}");
                return false;
            }
        }


        // ✅ Update an existing payment profile
        public async Task<bool> UpdateOwnerPaymentProfileAsync(PaymentProfile profile)
        {
            try
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating owner payment profile: {ex.Message}");
                return false;
            }
        }

        // ✅ Update a specific payment profile by ID
        public async Task<bool> UpdateAsync(PaymentProfile profile)
        {
            try
            {
                _dbSet.Update(profile);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating payment profile: {ex.Message}");
                return false;
            }
        }

        // ✅ Delete a payment profile by ID
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var profile = await _dbSet.FindAsync(id);
                if (profile != null)
                {
                    _dbSet.Remove(profile);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting payment profile ID {id}: {ex.Message}");
                return false;
            }
        }
    }
}
