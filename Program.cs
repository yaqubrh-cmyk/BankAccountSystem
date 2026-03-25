using BankAccountSystem.Data.Context;
using BankAccountSystem.Models;
using BankAccountSystem.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;


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
    var exchangeService = new ExchangeService();

    bool preserveOutput = false; // if true, don't clear console between menu iterations
    var allowedCurrencies = new[] { "AZN", "USD", "EUR" };

    while (true)
    {
        if (!preserveOutput)
        {
            Console.Clear();
        }
        else
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("-- Ekran qorunur. Ekranı təmizləmək üçün menyudan 7 seçin --");
            Console.ResetColor();
        }

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
        Console.WriteLine("6. 🗑 Müştəri Sil");
        Console.WriteLine("7. 🧹 Ekranı Təmizlə (qorunan çıxarılsın)");
        Console.WriteLine("8. 💱 Valyuta çevirmə göstərin");
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

                string ad;
                while (true)
                {
                    Console.Write("Ad və Soyad: ");
                    ad = Console.ReadLine()!.Trim();
                    if (string.IsNullOrEmpty(ad))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ad boş ola bilməz. Yenidən daxil edin.");
                        Console.ResetColor();
                        continue;
                    }
                    break;
                }

                string fin;
                // FIN format: AZE followed by 7 digits (e.g. AZE1234567)
                var finPattern = new Regex("^AZE\\d{7}$", RegexOptions.IgnoreCase);
                while (true)
                {
                    Console.Write("FİN Kod (məsələn AZE1234567): ");
                    fin = Console.ReadLine()!.Trim();
                    if (!finPattern.IsMatch(fin))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FİN formatı düzgün deyil. Format: 'AZE' sonra 7 rəqəm (məsələn AZE1234567). Yenidən cəhd edin.");
                        Console.ResetColor();
                        continue;
                    }
                    break;
                }

                string email;
                while (true)
                {
                    Console.Write("Email ünvanı: ");
                    email = Console.ReadLine()!.Trim();
                    if (string.IsNullOrEmpty(email))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Email boş ola bilməz. Yenidən daxil edin.");
                        Console.ResetColor();
                        continue;
                    }
                    if (!email.Contains("@") || !email.Contains("."))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Email '@' və domen hissəsi (məs. example.com) olmadan qəbul edilmir. Yenidən cəhd edin.");
                        Console.ResetColor();
                        continue;
                    }
                    break;
                }

                string phone;
                // Phone format: +994 followed by 9 digits
                var phonePattern = new Regex("^\\+994\\d{9}$");
                while (true)
                {
                    Console.Write("Telefon nömrəsi (məs. +994501234567): ");
                    phone = Console.ReadLine()!.Trim();
                    if (!phonePattern.IsMatch(phone))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Telefon nömrəsi düzgün formatda deyil. Format: +994 və 9 rəqəm (məs. +994501234567). Yenidən cəhd edin.");
                        Console.ResetColor();
                        continue;
                    }
                    break;
                }

                DateTime dob;
                while (true)
                {
                    Console.Write("Doğum tarixi (YYYY-MM-DD): ");
                    string dobInput = Console.ReadLine()!;
                    if (DateTime.TryParseExact(dobInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dob))
                    {
                        // valid parse
                    }
                    else if (DateTime.TryParse(dobInput, out dob))
                    {
                        // valid parse
                    }
                    else
                    {
                        Console.WriteLine("Tarix formatı düzgün deyil. Yenidən cəhd edin.");
                        continue;
                    }

                    // age check
                    var today = DateTime.Today;
                    var age = today.Year - dob.Year;
                    if (dob > today.AddYears(-age)) age--;
                    if (age < 18)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("18 yaşından kiçik şəxslər qeydiyyatdan keçə bilməz. Yenidən tarixi daxil edin.");
                        Console.ResetColor();
                        continue;
                    }

                    break;
                }

                var newCustomer = new Customer { FullName = ad, NationalId = fin, Email = email, Phone = phone, DateOfBirth = dob };
                await customerService.AddCustomerAsync(newCustomer);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Müştəri uğurla bazaya əlavə edildi!");
                Console.WriteLine($"ID: {newCustomer.Id} | Ad: {newCustomer.FullName} | FİN: {newCustomer.NationalId} | Telefon: {newCustomer.Phone} | Doğum: {newCustomer.DateOfBirth:yyyy-MM-dd}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                preserveOutput = true;
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
                preserveOutput = true;
                break;

            case "3":
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("\nMüştəri ID-sini daxil edin: ");
                int mId = int.Parse(Console.ReadLine()!);

                string valyuta;
                while (true)
                {
                    Console.Write("Valyuta seçin (AZN, USD, EUR): ");
                    valyuta = Console.ReadLine()!.ToUpper().Trim();
                    if (!allowedCurrencies.Contains(valyuta))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Dəstəklənməyən valyuta. Yenidən seçim edin.");
                        Console.ResetColor();
                        continue;
                    }
                    break;
                }

                // Show conversion rates for 1 unit of chosen currency
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n1 {valyuta} aşağıdakı valyutalara bərabərdir:");
                var rates = exchangeService.GetRatesFor(valyuta);
                foreach (var r in rates)
                {
                    Console.WriteLine($" - 1 {valyuta} = {r.Value} {r.Key}");
                }
                Console.ResetColor();

                // Optionally allow user to convert a custom amount before creating account
                Console.Write("\nMəbləği daxil edin (məsələn 1) və Enter düyməsini basın və ya boş buraxın: ");
                var amountInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(amountInput) && decimal.TryParse(amountInput, out var amountToConvert))
                {
                    Console.WriteLine($"\n{amountToConvert} {valyuta} üçün konvertasiya:");
                    foreach (var target in allowedCurrencies)
                    {
                        var converted = exchangeService.Convert(valyuta, target, amountToConvert);
                        Console.WriteLine($" - {converted} {target}");
                    }
                }

                var createdAccount = await accountService.CreateAccountAsync(mId, valyuta);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✅ {valyuta} hesabı uğurla açıldı!");
                Console.WriteLine($"Hesab ID: {createdAccount.Id} | Nº: {createdAccount.AccountNumber} | Valyuta: {createdAccount.Currency} | Balans: {createdAccount.Balance}");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                preserveOutput = true;
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
                preserveOutput = true;
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
                preserveOutput = true;
                break;

            case "6":
                Console.Write("\nSilmək istədiyiniz müştərinin ID-sini daxil edin: ");
                if (!int.TryParse(Console.ReadLine(), out int deleteId))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Düzgün ID daxil edin.");
                    Console.ResetColor();
                    Console.WriteLine("Davams etmək üçün düyməyə basın...");
                    Console.ReadKey();
                    preserveOutput = true;
                    break;
                }
                var customer = await db.Customers.FindAsync(deleteId);
                if (customer == null || customer.IsDeleted)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Müştəri tapılmadı.");
                    Console.ResetColor();
                    Console.WriteLine("Davams etmək üçün düyməyə basın...");
                    Console.ReadKey();
                    preserveOutput = true;
                    break;
                }
                await customerService.DeleteCustomerAsync(deleteId);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Müştəri uğurla silindi (soft-delete). ");
                Console.ResetColor();
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                preserveOutput = true;
                break;

            case "7":
                // clear preserved output and console
                preserveOutput = false;
                Console.Clear();
                Console.WriteLine("Ekran təmizləndi. Davam etmək üçün Enter basın...");
                Console.ReadKey();
                break;

            case "8":
                Console.Write("\nBasis valyutanı daxil edin (AZN, USD, EUR): ");
                var baseCur = Console.ReadLine()!.ToUpper().Trim();
                if (!allowedCurrencies.Contains(baseCur))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Dəstəklənməyən valyuta.");
                    Console.ResetColor();
                    Console.WriteLine("Davams etmək üçün düyməyə basın...");
                    Console.ReadKey();
                    preserveOutput = true;
                    break;
                }
                var table = exchangeService.GetRatesFor(baseCur);
                Console.WriteLine($"\n1 {baseCur} =");
                foreach (var t in table)
                {
                    Console.WriteLine($" - {t.Value} {t.Key}");
                }
                Console.WriteLine("\nDavams etmək üçün düyməyə basın...");
                Console.ReadKey();
                preserveOutput = true;
                break;

            case "0":
                return;

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Yanlış seçim!");
                Console.WriteLine("Davams etmək üçün düyməyə basın...");
                Console.ReadKey();
                preserveOutput = true;
                break;
        }
    }
}
