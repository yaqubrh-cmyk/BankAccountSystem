using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountSystem.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = null!; 
        public int CustomerId { get; set; }
        public AccountType Type { get; set; }
        public decimal Balance { get; set; } = 0;
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public List<Transaction> Transactions { get; set; } = new();
        public string Currency { get; set; } = "AZN"; // Default olaraq AZN
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
