using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using BankAccountSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// --- HELPER VISUAL FUNCTIONS (top-level) ---
void PrintHeader(string title)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    string line = new string('═', 60);
    string space = new string(' ', Math.Max(0, (60 - title.Length) / 2));

    Console.WriteLine($"╔{line}╗");
    Console.WriteLine($"║{space}{title.ToUpper()}{new string(' ', Math.Max(0, 60 - space.Length - title.Length))}║");
    Console.WriteLine($"╚{line}╝");
    Console.ResetColor();
}

void PrintSection(string title)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Black;
    Console.BackgroundColor = ConsoleColor.Yellow;
    Console.Write($" ► {title} ");
    Console.ResetColor();
    Console.WriteLine("\n" + new string('-', 40));
}

void SuccessMessage(string msg)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n [✔] {msg}");
    Console.ResetColor();
}

void ErrorMessage(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n [✘] XƏTA: {msg}");
    Console.ResetColor();
}

void Pause()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(" ▐ Devam etmək üçün hər hansı bir düyməni sıxın...");
    Console.ResetColor();
    Console.ReadKey();
}

string FormatCell(string text, int width)
{
    text ??= string.Empty;
    if (text.Length > width) return text.Substring(0, Math.Max(0, width - 3)) + "...";
    return text.PadRight(width);
}

void PrintTableHeader(params string[] columns)
{
    Console.BackgroundColor = ConsoleColor.DarkGray;
    Console.ForegroundColor = ConsoleColor.White;
    string row = "";
    int[] widths = { 8, 25, 12, 15, 12 }; // ID, Ad, Fin, Tel, Tarix üçün təxmini

    for (int i = 0; i < columns.Length; i++)
    {
        row += FormatCell(columns[i], (i < widths.Length ? widths[i] : 15));
    }
    Console.WriteLine(row);
    Console.ResetColor();
}

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

// --- rest of file (RunMainMenu and other functions) ---
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
        PrintHeader("BANK İDARƏETMƏ SİSTEMİ — ƏSAS MENYU");
        Console.WriteLine("1. Müştəri Konsolu — müştəri qeydiyyatı və idarəetmə");
        Console.WriteLine("2. Hesab Konsolu — hesab açma və əməliyyatlar");
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
                Pause();
                break;
        }
    }
}

// (The rest of the file starting at line 102 remains unchanged)

async Task RunCustomerConsole(BankContext db, CustomerService customerService)
{
    while (true)
    {
        PrintHeader("MÜŞTƏRİ KONSOLU");
        Console.WriteLine("1. Yeni müştəri qeydiyyatı");
        Console.WriteLine("2. Müştərilərin siyahısı");
        Console.WriteLine("3. Müştəriyə aid hesabları göstər");
        Console.WriteLine("4. Müştəri sil (hesablar və əməliyyatlar da silinəcək)");
        Console.WriteLine("5. Müştəri məlumatlarını yenilə");
        Console.WriteLine("0. Əsas menyuya qayıt");
        Console.Write("Seçiminizi edin: ");

        var s = Console.ReadLine();
        if (s == "0") return;

        switch (s)
        {
            case "1":
                await AddCustomerFlow(customerService);
                break;
            case "2":
                PrintSection("Müştərilər — Siyahı");
                var list = await customerService.GetAllCustomersAsync();
                if (!list.Any()) Console.WriteLine("Heç bir müştəri yoxdur.");
                else
                {
                    // Table header
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(FormatCell("ID", 5) + FormatCell("Ad Soyad", 30) + FormatCell("FİN", 12) + FormatCell("Telefon", 15) + FormatCell("Doğum", 12));
                    Console.ResetColor();
                    foreach (var m in list)
                    {
                        Console.WriteLine(FormatCell(m.Id.ToString(), 5) + FormatCell(m.FullName, 30) + FormatCell(m.NationalId, 12) + FormatCell(m.Phone, 15) + FormatCell(m.DateOfBirth.ToString("yyyy-MM-dd"), 12));
                    }
                }
                Pause();
                break;
            case "3":
                Console.Write("Müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var cid)) { Console.WriteLine("Düzgün ID daxil edin."); Pause(); break; }
                var cust = await db.Customers.Include(c => c.Accounts).ThenInclude(a => a.Transactions).FirstOrDefaultAsync(c => c.Id == cid);
                if (cust == null || cust.IsDeleted) { Console.WriteLine("Müştəri tapılmadı və ya silinib."); Pause(); break; }

                PrintSection($"{cust.FullName} — Hesabları");
                if (cust.Accounts == null || !cust.Accounts.Any()) Console.WriteLine("Bu müştəriyə aid heç bir hesab yoxdur.");
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(FormatCell("HesabID", 8) + FormatCell("Nömrə", 18) + FormatCell("Valyuta", 8) + FormatCell("Balans", 12) + FormatCell("Əməliyyatlar", 12));
                    Console.ResetColor();
                    foreach (var acc in cust.Accounts)
                    {
                        Console.WriteLine(FormatCell(acc.Id.ToString(), 8) + FormatCell(acc.AccountNumber, 18) + FormatCell(acc.Currency, 8) + FormatCell(acc.Balance.ToString("F2"), 12) + FormatCell((acc.Transactions?.Count ?? 0).ToString(), 12));
                    }
                }

                Console.Write("\nHesab ID daxil edin ətraflı əməliyatlar üçün (boş buraxın qayıt): ");
                var accIn = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(accIn) && int.TryParse(accIn, out var aid))
                {
                    var account = cust.Accounts.FirstOrDefault(a => a.Id == aid);
                    if (account == null) { Console.WriteLine("Hesab tapılmadı."); Pause(); break; }
                    PrintSection($"Hesab {account.AccountNumber} — Əməliyyatlar");
                    if (account.Transactions == null || !account.Transactions.Any()) Console.WriteLine("Bu hesabda əməliyyat yoxdur.");
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(FormatCell("ID",6) + FormatCell("Tip",10) + FormatCell("Məbləğ",12) + FormatCell("BalansSonra",12) + FormatCell("Tarix",20));
                        Console.ResetColor();
                        foreach (var t in account.Transactions.OrderByDescending(t => t.OccurredAt))
                        {
                            Console.WriteLine(FormatCell(t.Id.ToString(),6) + FormatCell(t.TransactionType,10) + FormatCell(t.Amount.ToString("F2"),12) + FormatCell(t.BalanceAfter.ToString("F2"),12) + FormatCell(t.OccurredAt.ToString("yyyy-MM-dd HH:mm"),20));
                        }
                    }
                }
                Pause();
                break;
            case "4":
                Console.Write("Silmək istədiyiniz müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var delId)) { Console.WriteLine("Düzgün ID daxil edin."); Pause(); break; }
                await customerService.DeleteCustomerAsync(delId);
                Console.WriteLine("Müştəri silindi (hesablar və əməliyyatlar silindi).");
                Pause();
                break;
            case "5":
                Console.Write("Yeniləmək istədiyiniz müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var updId)) { Console.WriteLine("Düzgün ID daxil edin."); Pause(); break; }
                var customerToUpdate = await customerService.GetCustomerByIdAsync(updId);
                if (customerToUpdate == null) { Console.WriteLine("Müştəri tapılmadı."); Pause(); break; }

                // Update loop
                while (true)
                {
                    PrintSection($"Müştəri Məlumatları — ID: {customerToUpdate.Id}");
                    Console.WriteLine($"Ad: {customerToUpdate.FullName}");
                    Console.WriteLine($"FİN: {customerToUpdate.NationalId}");
                    Console.WriteLine($"Telefon: {customerToUpdate.Phone}");
                    Console.WriteLine($"Doğum Tarixi: {customerToUpdate.DateOfBirth:yyyy-MM-dd}");
                    Console.WriteLine($"Email: {customerToUpdate.Email}");
                    Console.WriteLine("-------------------");

                    Console.WriteLine("Düzəliş etmək istədiyiniz sahəni seçin:");
                    Console.WriteLine("1. Ad");
                    Console.WriteLine("2. Telefon");
                    Console.WriteLine("3. Email");
                    Console.WriteLine("0. Qayıt");
                    Console.Write("Seçiminizi edin: ");

                    var fieldChoice = Console.ReadLine();
                    if (fieldChoice == "0") break;

                    string newValue;
                    switch (fieldChoice)
                    {
                        case "1":
                            Console.Write("Yeni Ad və Soyad: ");
                            newValue = Console.ReadLine()!.Trim();
                            if (string.IsNullOrEmpty(newValue)) { Console.WriteLine("Ad boş ola bilməz."); continue; }
                            customerToUpdate.FullName = newValue;
                            break;
                        case "2":
                            Console.Write("Yeni Telefon (məsələn +994501234567): ");
                            newValue = Console.ReadLine()!.Trim();
                            if (string.IsNullOrEmpty(newValue)) break;
                            if (newValue.StartsWith("+994") && newValue.Length == 13) customerToUpdate.Phone = newValue;
                            else if (newValue.StartsWith("051") && newValue.Length == 9) customerToUpdate.Phone = "+994" + newValue;
                            else { Console.WriteLine("Telefon nömrəsi düzgün deyil. Beynəlxalq formatda (+994...) daxil edin."); continue; }
                            break;
                        case "3":
                            Console.Write("Yeni Email ünvanı: ");
                            newValue = Console.ReadLine()!.Trim();
                            if (string.IsNullOrEmpty(newValue)) { Console.WriteLine("Email boş ola bilməz."); continue; }
                            customerToUpdate.Email = newValue;
                            break;
                        default:
                            Console.WriteLine("Yanlış seçim.");
                            continue;
                    }

                    // Save changes
                    await customerService.UpdateCustomerAsync(customerToUpdate);
                    SuccessMessage("Müştəri məlumatları yeniləndi.");
                    Pause();
                }

                break;
            default:
                Console.WriteLine("Yanlış seçim.");
                Pause();
                break;
        }
    }
}

async Task AddCustomerFlow(CustomerService customerService)
{
    PrintHeader("Yeni Müştəri Qeydiyyatı");

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
        if (string.IsNullOrEmpty(email)) { Console.WriteLine("Email boş ola bilməz."); continue; }
        if (!email.Contains("@")) { Console.WriteLine("Email '@' olmadan qəbul edilmir."); continue; }
        break;
    }

    string? tel = null;
    while (true)
    {
        Console.Write("Telefon (məsələn +994501234567): ");
        tel = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(tel)) break;
        if (tel.StartsWith("+994") && tel.Length == 13) break;
        if (tel.StartsWith("051") && tel.Length == 9) { tel = "+994" + tel; break; }
        Console.WriteLine("Telefon nömrəsi düzgün deyil. Beynəlxalq formatda (+994...) daxil edin.");
    }

    DateTime dob;
    while (true)
    {
        Console.Write("Doğum tarixi (yyyy-MM-dd): ");
        var dobInput = Console.ReadLine()!.Trim();
        if (!DateTime.TryParseExact(dobInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob) && !DateTime.TryParse(dobInput, out dob)) { Console.WriteLine("Tarix düzgün deyil. Tam il-aa-gg formatında daxil edin."); continue; }
        var today = DateTime.Today; var age = today.Year - dob.Year; if (dob > today.AddYears(-age)) age--; if (age < 18) { Console.WriteLine("18 yaşdan kiçik qeydiyyat edə bilməz."); continue; }
        break;
    }

    // Müştəri yaradılması
    var customer = new Customer
    {
        FullName = ad,
        NationalId = fin,
        Email = email,
        Phone = tel,
        DateOfBirth = dob
    };
    await customerService.AddCustomerAsync(customer);
    SuccessMessage("Müştəri uğurla yaradıldı.");

    Pause();
}

async Task RunAccountConsole(BankContext db, AccountService accountService, ExchangeService exchangeService, string[] allowedCurrencies)
{
    while (true)
    {
        PrintHeader("HESAB KONSOLU");
        Console.WriteLine("1. Hesab aç");
        Console.WriteLine("2. Mədaxil (Balans artır)");
        Console.WriteLine("3. Məxaric (Balansdan çıxar)");
        Console.WriteLine("4. Hesabın əməliyyatlarına bax");
        Console.WriteLine("5. Pul köçürməsi");
        Console.WriteLine("6. Bütün müştərilərin hesabları (hamısını göstər)");
        Console.WriteLine("0. Əsas menyuya qayıt");
        Console.Write("Seçiminizi edin: ");

        var s = Console.ReadLine();
        if (s == "0") return;

        switch (s)
        {
            case "1":
                Console.Write("Müştəri ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var cid)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                Console.Write("Valyuta (AZN, USD, EUR): ");
                var cur = Console.ReadLine()!.ToUpper().Trim();
                if (!allowedCurrencies.Contains(cur)) { ErrorMessage("Dəstəklənməyən valyuta."); Pause(); break; }
                try
                {
                    var acc = await accountService.CreateAccountAsync(cid, cur);
                    SuccessMessage($"Hesab yaradıldı. ID:{acc.Id}   Nº:{acc.AccountNumber}   Valyuta:{acc.Currency}");
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
                Pause();
                break;
            case "2":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                Console.Write("Mədaxil məbləği: ");
                if (!decimal.TryParse(Console.ReadLine(), out var amt)) { ErrorMessage("Düzgün məbləğ daxil edin."); Pause(); break; }
                try
                {
                    var acc = await accountService.DepositAsync(aid, amt);
                    SuccessMessage($"Uğurla əlavə olundu. Yeni balans: {acc.Balance:F2}");
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
                Pause();
                break;
            case "3":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid2)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                Console.Write("Çıxarılacaq məbləğ: ");
                if (!decimal.TryParse(Console.ReadLine(), out var amt2)) { ErrorMessage("Düzgün məbləğ daxil edin."); Pause(); break; }
                try
                {
                    var acc = await accountService.WithdrawAsync(aid2, amt2);
                    SuccessMessage($"Uğurla çıxdı. Yeni balans: {acc.Balance:F2}");
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
                Pause();
                break;
            case "4":
                Console.Write("Hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var aid3)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                var account = await db.Accounts.Include(a => a.Transactions).FirstOrDefaultAsync(a => a.Id == aid3);
                if (account == null) { ErrorMessage("Hesab tapılmadı."); Pause(); break; }
                PrintSection($"Hesab: {account.AccountNumber}");
                Console.WriteLine($"Valyuta: {account.Currency}   Balans: {account.Balance:F2}   Status: {account.Status}");
                if (account.Transactions == null || !account.Transactions.Any()) Console.WriteLine("Əməliyyat yoxdur.");
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(FormatCell("ID",6) + FormatCell("Tip",10) + FormatCell("Məbləğ",12) + FormatCell("SonBalans",12) + FormatCell("Tarix",20));
                    Console.ResetColor();
                    foreach (var t in account.Transactions.OrderByDescending(t => t.OccurredAt))
                        Console.WriteLine(FormatCell(t.Id.ToString(),6) + FormatCell(t.TransactionType,10) + FormatCell(t.Amount.ToString("F2"),12) + FormatCell(t.BalanceAfter.ToString("F2"),12) + FormatCell(t.OccurredAt.ToString("yyyy-MM-dd HH:mm"),20));
                }
                Pause();
                break;
            case "5":
                Console.Write("Göndərən hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var senderId)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                Console.Write("Alıcı hesab ID daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out var receiverId)) { ErrorMessage("Düzgün ID daxil edin."); Pause(); break; }
                Console.Write("Köçürmə məbləğı: ");
                if (!decimal.TryParse(Console.ReadLine(), out var transferAmt)) { ErrorMessage("Düzgün məbləğ daxil edin."); Pause(); break; }
                try
                {
                    var result = await accountService.TransferBetweenAccountsAsync(senderId, receiverId, transferAmt);
                    SuccessMessage($"Uğurla köçürüldü. Yeni Balans - Göndərən: {result.SenderNewBalance:F2}, Alıcı: {result.ReceiverNewBalance:F2}");
                }
                catch (Exception ex)
                {
                    ErrorMessage(ex.Message);
                }
                Pause();
                break;
            case "6":
                // load all accounts and customers to avoid N+1
                var allAccounts = await db.Accounts.Include(a => a.Transactions).ToListAsync();
                var customerIds = allAccounts.Select(a => a.CustomerId).Distinct().ToList();
                var customersDict = await db.Customers.Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

                if (!allAccounts.Any())
                {
                    Console.WriteLine("Heç bir hesab tapılmadı.");
                    Pause();
                    break;
                }

                PrintSection("Bütün Müştərilərin Hesabları");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(FormatCell("AccID",6) + FormatCell("Nömrə",18) + FormatCell("Müştəri",25) + FormatCell("Valyuta",8) + FormatCell("Balans",12) + FormatCell("Əməliyyatlar",12) + FormatCell("Status",10));
                Console.ResetColor();

                foreach (var acc in allAccounts)
                {
                    customersDict.TryGetValue(acc.CustomerId, out var owner);
                    var ownerName = owner != null ? owner.FullName : "Unknown";
                    Console.WriteLine(FormatCell(acc.Id.ToString(),6) + FormatCell(acc.AccountNumber,18) + FormatCell(ownerName,25) + FormatCell(acc.Currency,8) + FormatCell(acc.Balance.ToString("F2"),12) + FormatCell((acc.Transactions?.Count ?? 0).ToString(),12) + FormatCell(acc.Status.ToString(),10));
                }
                Pause();
                break;
            default:
                ErrorMessage("Yanlış seçim.");
                Pause();
                break;
        }
    }
}

async Task RunExchangeConsole(ExchangeService exchangeService, string[] allowedCurrencies)
{
    PrintHeader("Valyuta Çevirmə");
    Console.Write("Basis valyutanı daxil edin (AZN, USD, EUR): ");
    var baseCur = Console.ReadLine()!.ToUpper().Trim();
    if (!allowedCurrencies.Contains(baseCur)) { ErrorMessage("Dəstəklənməyən valyuta."); Pause(); return; }
    var table = exchangeService.GetRatesFor(baseCur);
    Console.WriteLine($"\n1 {baseCur} =");
    foreach (var t in table) Console.WriteLine($" - {t.Value} {t.Key}");
    Pause();
}