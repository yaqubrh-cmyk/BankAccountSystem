using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using BankAccountSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

Console.OutputEncoding = System.Text.Encoding.UTF8;

try
{
    await RunMainMenu();
}
catch (Exception ex)
{
    Console.WriteLine($"🛑 Kritik Xəta: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Daxili Xəta: {ex.InnerException.Message}");
    Console.WriteLine("\nProqram dayandı. Davam etmək üçün düymə sıxın...");
    Console.ReadKey();
}

async Task RunMainMenu()
{
    var optionsBuilder = new DbContextOptionsBuilder<BankContext>();
    optionsBuilder.UseSqlServer("Server=DESKTOP-3GNUE7J\\SQLEXPRESS;Database=BankAccountSystemDb;Trusted_Connection=True;TrustServerCertificate=true");

    using var db = new BankContext(optionsBuilder.Options);
    var customerService = new CustomerService(db);
    var accountService = new AccountService(db);
    var exchangeService = new ExchangeService();

    var allowedCurrencies = new[] { "AZN", "USD", "EUR" };

    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.WriteLine("         BANK İDARƏETMƏ SİSTEMİ 🏦");
        Console.WriteLine("========================================");
        Console.ResetColor();

        Console.WriteLine("1. Müştəri Konsolu (Müştəri qeydiyyatı / idarəetmə)");
        Console.WriteLine("2. Hesab Konsolu (Hesab açma / əməliyyatlar)");
        Console.WriteLine("3. Valyuta çevirmə");
        Console.WriteLine("0. Çıxış");
        Console.Write("Seçiminizi edin: ");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await RunCustomerConsole(db, customerService);
                break;
            case "2":
                await RunAccountConsole(db, accountService, exchangeService, allowedCurrencies);
                break;
            case "3":
                await RunExchangeConsole(exchangeService, allowedCurrencies);
                break;
            case "0":
                return;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Yanlış seçim. Yenidən cəhd edin.");
                Console.ResetColor();
                Console.WriteLine("Davam etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;
        }
    }
}

async Task RunCustomerConsole(BankContext db, CustomerService customerService)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("--- Müştəri Konsolu ---");
        Console.WriteLine("1. Yeni müştəri qeydiyyatı");
        Console.WriteLine("2. Müştərilərin siyahısı");
        Console.WriteLine("3. Müştəriyə aid hesabları göstər");
        Console.WriteLine("4. Müştəri sil (hesablar və əməliyyatlar da silinəcək)");
        Console.WriteLine("5. Əsas menyuya qayıt");
        Console.Write("Seçiminizi edin: ");

        var s = Console.ReadLine();
        if (s == "5") return;

        switch (s)
        {
            case "1":
                await AddCustomerFlow(customerService);
                break;
            case "2":
                var list = await customerService.GetAllCustomersAsync();
                Console.WriteLine("\nMüştərilər:");
                foreach (var m in list)
                {
                    Console.WriteLine($"ID:{m.Id} | {m.FullName} | FİN:{m.NationalId} | Telefon:{m.Phone} | Doğum:{m.DateOfBirth:yyyy-MM-dd}");
                }
                Console.WriteLine("\nDavam etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;
            case "3":
                Console.Write("Müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var cid))
                {
                    Console.WriteLine("Düzgün ID daxil edin.");
                    Console.ReadKey();
                    break;
                }
                var cust = await db.Customers.Include(c => c.Accounts).ThenInclude(a => a.Transactions).FirstOrDefaultAsync(c => c.Id == cid);
                if (cust == null || cust.IsDeleted)
                {
                    Console.WriteLine("Müştəri tapılmadı və ya silinib.");
                    Console.ReadKey();
                    break;
                }
                Console.WriteLine($"\n{cust.FullName} adlı müştərinin hesabları (ümumi {cust.Accounts.Count}):");
                foreach (var acc in cust.Accounts)
                {
                    Console.WriteLine($"Hesab ID:{acc.Id} | Nº:{acc.AccountNumber} | Valyuta:{acc.Currency} | Balans:{acc.Balance} | Əməliyyatlar:{acc.Transactions?.Count ?? 0}");
                }
                Console.WriteLine("\nHesabın əməliyyatlarını görmək üçün Hesab ID daxil edin (boş buraxın geri qayıt): ");
                var accIn = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(accIn) && int.TryParse(accIn, out var aid))
                {
                    var account = cust.Accounts.FirstOrDefault(a => a.Id == aid);
                    if (account == null)
                    {
                        Console.WriteLine("Hesab tapılmadı.");
                      }
                      else
                      {
                        if (account.Transactions == null || !account.Transactions.Any())
                        {
                            Console.WriteLine("Bu hesabda əməliyyat yoxdur.");
                        }
                        else
                        {
                            foreach (var t in account.Transactions.OrderByDescending(t => t.OccurredAt))
                            {
                                Console.WriteLine($"ID:{t.Id} | Tip:{t.TransactionType} | Məbləğ:{t.Amount} | BalansSonra:{t.BalanceAfter} | Tarix:{t.OccurredAt:yyyy-MM-dd HH:mm}");
                            }
                        }
                      }
                }
                Console.WriteLine("\nDavam etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;
            case "4":
                Console.Write("Silmək istədiyiniz müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var delId))
                {
                    Console.WriteLine("Düzgün ID daxil edin.");
                    Console.ReadKey();
                    break;
                }
                await customerService.DeleteCustomerAsync(delId);
                Console.WriteLine("Müştəri silindi (hesablar və əməliyyatlar silindi).");
                Console.ReadKey();
                break;
            default:
                Console.WriteLine("Yanlış seçim.");
                Console.ReadKey();
                break;
        }
    }
}

async Task AddCustomerFlow(CustomerService customerService)
{
    string ad;
    while (true)
    {
        Console.Write("Ad və Soyad: ");
        ad = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(ad)) { Console.WriteLine("Ad boş ola bilməz."); continue; }
        break;
    }

    string fin;
    var finPattern = new Regex("^AZE\\d{7}$", RegexOptions.IgnoreCase);
    while (true)
    {
        Console.Write("FİN Kod (məsələn AZE1234567): ");
        fin = Console.ReadLine()!.Trim();
        if (!finPattern.IsMatch(fin)) { Console.WriteLine("FİN formatı düzgün deyil."); continue; }
        break;
    }

    string email;
    while (true)
    {
        Console.Write("Email ünvanı: ");
        email = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains(".")) { Console.WriteLine("Email düzgün deyil."); continue; }
        break;
    }

    string phone;
    var phonePattern = new Regex("^\\+994\\d{9}$");
    while (true)
    {
        Console.Write("Telefon nömrəsi (məs. +994501234567): ");
        phone = Console.ReadLine()!.Trim();
        if (!phonePattern.IsMatch(phone)) { Console.WriteLine("Telefon formatı düzgün deyil."); continue; }
        break;
    }

    DateTime dob;
    while (true)
    {
        Console.Write("Doğum tarixi (YYYY-MM-DD): ");
        var dobInput = Console.ReadLine()!;
        if (!DateTime.TryParseExact(dobInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob) && !DateTime.TryParse(dobInput, out dob)) { Console.WriteLine("Tarix düzgün deyil."); continue; }
        var today = DateTime.Today; var age = today.Year - dob.Year; if (dob > today.AddYears(-age)) age--; if (age < 18) { Console.WriteLine("18 yaşdan kiçik qeydiyyat edə bilməz."); continue; }
        break;
    }

    var newCustomer = new Customer { FullName = ad, NationalId = fin, Email = email, Phone = phone, DateOfBirth = dob };
    await customerService.AddCustomerAsync(newCustomer);
    Console.WriteLine($"Müştəri yaradıldı. ID: {newCustomer.Id}");
    Console.WriteLine("Davam etmək üçün düyməyə basın...");
    Console.ReadKey();
}

async Task RunAccountConsole(BankContext db, AccountService accountService, ExchangeService exchangeService, string[] allowedCurrencies)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("--- Hesab Konsolu ---");
        Console.WriteLine("1. Hesab aç");
        Console.WriteLine("2. Mədaxil (Balans artır)");
        Console.WriteLine("3. Məxaric (Balansdan çıxar)");
        Console.WriteLine("4. Hesabın əməliyyatlarına bax");
        Console.WriteLine("9. Əsas menyuya qayıt");
        Console.Write("Seçiminizi edin: ");

        var s = Console.ReadLine();
        if (s == "9") return;

        switch (s)
        {
            case "1":
                Console.Write("Müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var cid)) { Console.WriteLine("Düzgün ID daxil edin."); Console.ReadKey(); break; }
                Console.Write("Valyuta (AZN, USD, EUR): ");
                var cur = Console.ReadLine()!.ToUpper().Trim();
                if (!allowedCurrencies.Contains(cur)) { Console.WriteLine("Dəstəklənməyən valyuta."); Console.ReadKey(); break; }
                try
                {
                    var acc = await accountService.CreateAccountAsync(cid, cur);
                    Console.WriteLine($"Hesab yaradıldı. ID:{acc.Id} Nº:{acc.AccountNumber} Valyuta:{acc.Currency}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xəta: {ex.Message}");
                }
                Console.ReadKey();
                break;
            case "2":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid)) { Console.WriteLine("Düzgün ID daxil edin."); Console.ReadKey(); break; }
                Console.Write("Mədaxil məbləği: ");
                if (!decimal.TryParse(Console.ReadLine(), out var amt)) { Console.WriteLine("Düzgün məbləğ daxil edin."); Console.ReadKey(); break; }
                try
                {
                    var acc = await accountService.DepositAsync(aid, amt);
                    Console.WriteLine($"Uğurla əlavə olundu. Yeni balans: {acc.Balance}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xəta: {ex.Message}");
                }
                Console.ReadKey();
                break;
            case "3":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid2)) { Console.WriteLine("Düzgün ID daxil edin."); Console.ReadKey(); break; }
                Console.Write("Çıxarılacaq məbləğ: ");
                if (!decimal.TryParse(Console.ReadLine(), out var amt2)) { Console.WriteLine("Düzgün məbləğ daxil edin."); Console.ReadKey(); break; }
                try
                {
                    var acc = await accountService.WithdrawAsync(aid2, amt2);
                    Console.WriteLine($"Uğurla çıxdı. Yeni balans: {acc.Balance}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Xəta: {ex.Message}");
                }
                Console.ReadKey();
                break;
            case "4":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid3)) { Console.WriteLine("Düzgün ID daxil edin."); Console.ReadKey(); break; }
                var account = await db.Accounts.Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Id == aid3);
                if (account == null) { Console.WriteLine("Hesab tapılmadı."); Console.ReadKey(); break; }
                Console.WriteLine($"Hesab: {account.AccountNumber} | Balans: {account.Balance} | Valyuta: {account.Currency}");
                if (account.Transactions == null || !account.Transactions.Any()) Console.WriteLine("Əməliyyat yoxdur.");
                else
                {
                    foreach (var t in account.Transactions.OrderByDescending(t => t.OccurredAt))
                        Console.WriteLine($"ID:{t.Id} | Tip:{t.TransactionType} | Məbləğ:{t.Amount} | SonBalans:{t.BalanceAfter} | {t.OccurredAt:yyyy-MM-dd HH:mm}");
                }
                Console.ReadKey();
                break;
            default:
                Console.WriteLine("Yanlış seçim.");
                Console.ReadKey();
                break;
        }
    }
}

async Task RunExchangeConsole(ExchangeService exchangeService, string[] allowedCurrencies)
{
    Console.Clear();
    Console.Write("Basis valyutanı daxil edin (AZN, USD, EUR): ");
    var baseCur = Console.ReadLine()!.ToUpper().Trim();
    if (!allowedCurrencies.Contains(baseCur)) { Console.WriteLine("Dəstəklənməyən valyuta."); Console.ReadKey(); return; }
    var table = exchangeService.GetRatesFor(baseCur);
    Console.WriteLine($"\n1 {baseCur} =");
    foreach (var t in table) Console.WriteLine($" - {t.Value} {t.Key}");
    Console.WriteLine("\nDavam etmək üçün düyməyə basın...");
    Console.ReadKey();
}
