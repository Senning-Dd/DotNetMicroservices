using System.Collections.Generic;

namespace PricingService.Domain;

public sealed class TariffPrice
{
    public TariffPrice(decimal totalPremium, IReadOnlyDictionary<string, CoverPrice> covers)
    {
        TotalPremium = totalPremium;
        Covers = covers;
    }

    public decimal TotalPremium { get; }
    public IReadOnlyDictionary<string, CoverPrice> Covers { get; }
}

public sealed class CoverPrice
{
    public CoverPrice(decimal price)
    {
        Price = price;
    }

    public decimal Price { get; }
}
