using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankAccountSystem.Services;

public class AccountService
{
    private readonly BankContext _context;

    public AccountService(BankContext context)
    {
        _context = context;
    }

    public async Task<Account> CreateAccountAsync(int customerId, string currency)
    {
        // Ensure customer exists and is not deleted
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null || customer.IsDeleted)
            throw new Exception("Müştəri tapılmadı və ya silinib. Hesab açıla bilməz.");

        var account = new Account
        {
            CustomerId = customerId,
            AccountNumber = "BA-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
            Balance = 0,
            Currency = currency.ToUpper(),
            CreatedAt = DateTime.Now
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> DepositAsync(int accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null) throw new Exception("Hesab tapılmadı!");

        // Check account status
        if (account.Status != AccountStatus.Active)
            throw new Exception("Bu hesab aktiv deyil. Əməliyyat icra edilə bilməz.");

        // Check customer
        var customer = await _context.Customers.FindAsync(account.CustomerId);
        if (customer == null || customer.IsDeleted)
            throw new Exception("Hesabın sahibi tapılmadı və ya silinib. Əməliyyat icra edilə bilməz.");

        account.Balance += amount;

        _context.Transactions.Add(new Transaction
        {
            AccountId = accountId,
            Amount = amount,
            TransactionType = "Deposit",
            Type = "Deposit",
            OccurredAt = DateTime.Now,
            BalanceAfter = account.Balance
        });

        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> WithdrawAsync(int accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);

        if (account == null)
            throw new Exception("Hesab tapılmadı!");

        // Check account status
        if (account.Status != AccountStatus.Active)
            throw new Exception("Bu hesab aktiv deyil. Əməliyyat icra edilə bilməz.");

        // Check customer
        var customer = await _context.Customers.FindAsync(account.CustomerId);
        if (customer == null || customer.IsDeleted)
            throw new Exception("Hesabın sahibi tapılmadı və ya silinib. Əməliyyat icra edilə bilməz.");

        if (account.Balance < amount)
            throw new Exception("Balansda kifayət qədər vəsait yoxdur!");

        account.Balance -= amount;

        _context.Transactions.Add(new Transaction
        {
            AccountId = accountId,
            Amount = -amount,
            TransactionType = "Withdraw",
            Type = "Withdraw",
            OccurredAt = DateTime.Now,
            BalanceAfter = account.Balance
        });

        await _context.SaveChangesAsync();
        return account;

    }
}