using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountSystem.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Type { get; set; } = null!; 
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.Now;
        public string TransactionType { get; set; } = null!; 
        
    }
}
