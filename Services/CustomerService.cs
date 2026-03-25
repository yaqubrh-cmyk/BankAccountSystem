using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using Microsoft.EntityFrameworkCore;

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
        return await _context.Customers.ToListAsync();
    }
    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            customer.IsDeleted = true; // Müəllimin istədiyi "Silinməsin, sadəcə gizlənsin" məntiqi
            await _context.SaveChangesAsync();
        }
    }
}