using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BankAccountSystem.Services;

public class CustomerService
{
    private readonly BankContext _context;

    public CustomerService(BankContext context)
    {
        _context = context;
    }
    public async Task AddCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        // Return only non-deleted customers
        return await _context.Customers.Where(c => !c.IsDeleted).ToListAsync();
    }
    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }
    public async Task UpdateCustomerAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
                .ThenInclude(a => a.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return;

        // Remove transactions and accounts belonging to customer
        if (customer.Accounts != null && customer.Accounts.Any())
        {
            foreach (var acc in customer.Accounts.ToList())
            {
                if (acc.Transactions != null && acc.Transactions.Any())
                {
                    _context.Transactions.RemoveRange(acc.Transactions);
                }
                _context.Accounts.Remove(acc);
            }
        }

        // Mark customer as deleted (soft delete) to keep record, but accounts removed
        customer.IsDeleted = true;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmailAsync(int id, string email)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || customer.IsDeleted)
            throw new Exception("Müştəri tapılmadı və ya silinib.");

        customer.Email = email;
        await _context.SaveChangesAsync();
    }
}