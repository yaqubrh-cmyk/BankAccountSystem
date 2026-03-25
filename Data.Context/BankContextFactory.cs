using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankAccountSystem.Data.Context
{
    public class BankContextFactory : IDesignTimeDbContextFactory<BankContext>
    {
        public BankContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BankContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-3GNUE7J\\SQLEXPRESS;Database=BankAccountSystemDb;Trusted_Connection=True;TrustServerCertificate=true");
            return new BankContext(optionsBuilder.Options);
        }
    }
}
