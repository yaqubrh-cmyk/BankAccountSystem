using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountSystem.Models
{
    public enum AccountType { Checking, Savings }
    public enum AccountStatus { Active, Closed }
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<Account> Accounts { get; set; } = new();
    }
}
