using System.Collections.Generic;

namespace BankAccountSystem.Services;

public class ExchangeService
{
    // Rates: 1 unit of currency equals X AZN
    // These are example static rates; update as needed or replace with an API.
    private readonly Dictionary<string, decimal> _toAzn = new()
    {
        { "AZN", 1m },
        { "USD", 1.7m },
        { "EUR", 1.9m }
    };

    public decimal Convert(string from, string to, decimal amount)
    {
        from = from.ToUpper();
        to = to.ToUpper();
        if (!_toAzn.ContainsKey(from) || !_toAzn.ContainsKey(to))
            throw new ArgumentException("Unsupported currency.");

        var amountInAzn = amount * _toAzn[from];
        return Math.Round(amountInAzn / _toAzn[to], 4);
    }

    public Dictionary<string, decimal> GetRatesFor(string currency)
    {
        currency = currency.ToUpper();
        if (!_toAzn.ContainsKey(currency))
            throw new ArgumentException("Unsupported currency.");

        var result = new Dictionary<string, decimal>();
        foreach (var kv in _toAzn)
        {
            result[kv.Key] = Convert(currency, kv.Key, 1m);
        }
        return result;
    }
}
