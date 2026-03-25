using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using BankAccountSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


Console.OutputEncoding = System.Text.Encoding.UTF8;

try
{
    await RunMenu();
}
catch (Exception ex)
{
    Console.WriteLine($"🛑 Kritik Xəta: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Daxili Xəta: {ex.InnerException.Message}");

    Console.WriteLine("\nProqram dayandı. Davam etmək üçün düymə sıxın...");
    Console.ReadKey();
}

async Task RunMenu()
{

    var optionsBuilder = new DbContextOptionsBuilder<BankContext>();
    optionsBuilder.UseSqlServer("Server=DESKTOP-3GNUE7J\\SQLEXPRESS;Database=BankAccountSystemDb;Trusted_Connection=True;TrustServerCertificate=true");

    using var db = new BankContext(optionsBuilder.Options);
    var customerService = new CustomerService(db);
    var accountService = new AccountService(db);
    while (true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan; // Başlıq rəngi
        Console.WriteLine("========================================");
        Console.WriteLine("       BANK İDARƏETMƏ SİSTEMİ 🏦");
        Console.WriteLine("========================================");

        Console.ForegroundColor = ConsoleColor.Yellow; // Menyu rəngi
        Console.WriteLine("1. ✨ Yeni Müştəri Qeydiyyatı");
        Console.WriteLine("2. 📋 Müştərilərin Siyahısı");
        Console.WriteLine("3. 💳 Yeni Bank Hesabı Aç (AZN/USD/EUR)");
        Console.WriteLine("4. 💰 Balansı Artır (Mədaxil)");
        Console.WriteLine("5. 💸 Balansdan Pul Çıxar (Məxaric)");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("0. ❌ Sistemdən Çıxış");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("----------------------------------------");
        Console.Write("Seçiminizi edin: ");

        string secim = Console.ReadLine()!;

        switch (secim)
        {
            case "1":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n--- 👤 Yeni Müştəri Məlumatları ---");
                Console.ResetColor();
                Console.Write("Ad və Soyad: ");
                string ad = Console.ReadLine()!;
                Console.Write("FİN Kod (7 simvol): ");
                string fin = Console.ReadLine()!;
                Console.Write("Email ünvanı: ");
                string email = Console.ReadLine()!;

                Console.Write("Telefon nömrəsi: ");
                string phone = Console.ReadLine()!;

                DateTime dob;
                while (true)
                {
                    Console.Write("Doğum tarixi (YYYY-MM-DD): ");
                    string dobInput = Console.ReadLine()!;
                    if (DateTime.TryParseExact(dobInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob))
                        break;
                    if (DateTime.TryParse(dobInput, out dob))
                        break;
                    Console.WriteLine("Tarix formatı düzgün deyil. Yenidən cəhd edin.");
                }

                var newCustomer = new Customer { FullName = ad, NationalId = fin, Email = email, Phone = phone, DateOfBirth = dob };
                await customerService.AddCustomerAsync(newCustomer);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Müştəri uğurla bazaya əlavə edildi!");
                Console.WriteLine($"ID: {newCustomer.Id} | Ad: {newCustomer.FullName} | FİN: {newCustomer.NationalId} | Telefon: {newCustomer.Phone} | Doğum: {newCustomer.DateOfBirth:yyyy-MM-dd}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;

            case "2":
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n--- 👥 Qeydiyyatdakı Müştərilər ---");
                Console.ResetColor();
                var musteriler = await customerService.GetAllCustomersAsync();
                foreach (var m in musteriler)
                {
                    Console.WriteLine($"🆔 ID: {m.Id} | 👤 Ad: {m.FullName} | 🔑 FİN: {m.NationalId} | 📞 Telefon: {m.Phone} | 🎂 Doğum tarixi: {m.DateOfBirth:yyyy-MM-dd}");
                }
                Console.WriteLine("\nDavams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;

            case "3":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("\nMüştəri ID-sini daxil edin: ");
                int mId = int.Parse(Console.ReadLine()!);
                Console.Write("Valyuta seçin (AZN, USD, EUR): ");
                string valyuta = Console.ReadLine()!.ToUpper();

                var createdAccount = await accountService.CreateAccountAsync(mId, valyuta);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ {valyuta} hesabı uğurla açıldı!");
                Console.WriteLine($"Hesab ID: {createdAccount.Id} | Nº: {createdAccount.AccountNumber} | Valyuta: {createdAccount.Currency} | Balans: {createdAccount.Balance}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;

            case "4":
                Console.Write("\nHesab ID-sini daxil edin: ");
                int hId = int.Parse(Console.ReadLine()!);
                Console.Write("Mədaxil ediləcək məbləğ: ");
                decimal mebleq = decimal.Parse(Console.ReadLine()!);

                var updatedAccountDeposit = await accountService.DepositAsync(hId, mebleq);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Balans artırıldı!");
                Console.WriteLine($"Hesab ID: {updatedAccountDeposit.Id} | Yeni balans: {updatedAccountDeposit.Balance}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;

            case "5":
                Console.Write("\nHesab ID-sini daxil edin: ");
                int mexId = int.Parse(Console.ReadLine()!);
                Console.Write("Çıxarılacaq məbləğ: ");
                decimal mexMebleq = decimal.Parse(Console.ReadLine()!);

                var updatedAccountWithdraw = await accountService.WithdrawAsync(mexId, mexMebleq);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Məbləğ uğurla çıxarıldı!");
                Console.WriteLine($"Hesab ID: {updatedAccountWithdraw.Id} | Yeni balans: {updatedAccountWithdraw.Balance}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;

            case "0":
                return;

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Yanlış seçim!");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                break;
        }
    }
}
