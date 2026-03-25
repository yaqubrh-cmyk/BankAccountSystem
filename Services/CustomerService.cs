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
    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.IsDeleted = true; // Soft-delete
            await _context.SaveChangesAsync();
        }
    }
}