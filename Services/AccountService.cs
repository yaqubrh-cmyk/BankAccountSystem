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

    public async Task<(Account FromAccount, Account ToAccount)> TransferAsync(int fromAccountId, int toAccountId, decimal amount)
    {
        if (fromAccountId == toAccountId)
            throw new Exception("Göndərilən və alıcı hesab eyni ola bilməz.");

        var from = await _context.Accounts.FindAsync(fromAccountId);
        var to = await _context.Accounts.FindAsync(toAccountId);

        if (from == null) throw new Exception("Göndərən hesab tapılmadı.");
        if (to == null) throw new Exception("Alıcı hesab tapılmadı.");

        if (from.Status != AccountStatus.Active || to.Status != AccountStatus.Active)
            throw new Exception("Hər iki hesab aktiv olmalıdır.");

        var custFrom = await _context.Customers.FindAsync(from.CustomerId);
        var custTo = await _context.Customers.FindAsync(to.CustomerId);
        if (custFrom == null || custFrom.IsDeleted) throw new Exception("Göndərənin sahibi tapılmadı və ya silinib.");
        if (custTo == null || custTo.IsDeleted) throw new Exception("Alıcının sahibi tapılmadı və ya silinib.");

        if (from.Balance < amount)
            throw new Exception("Göndərən hesabda kifayət qədər vəsait yoxdur.");

        // perform transfer
        from.Balance -= amount;
        to.Balance += amount;

        var now = DateTime.Now;

        _context.Transactions.Add(new Transaction
        {
            AccountId = from.Id,
            Amount = -amount,
            TransactionType = "TransferOut",
            Type = "Transfer",
            OccurredAt = now,
            BalanceAfter = from.Balance
        });

        _context.Transactions.Add(new Transaction
        {
            AccountId = to.Id,
            Amount = amount,
            TransactionType = "TransferIn",
            Type = "Transfer",
            OccurredAt = now,
            BalanceAfter = to.Balance
        });

        await _context.SaveChangesAsync();

        return (from, to);
    }

    public async Task<(decimal SenderNewBalance, decimal ReceiverNewBalance)> TransferBetweenAccountsAsync(int senderId, int receiverId, decimal amount)
    {
        var result = await TransferAsync(senderId, receiverId, amount);
        return (result.FromAccount.Balance, result.ToAccount.Balance);
    }
}