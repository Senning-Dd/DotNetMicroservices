using System.Collections.Generic;

namespace PricingService.Persistence;

internal sealed class TariffRecord
{
    public string Code { get; set; } = string.Empty;
    public ICollection<BasePremiumRuleRecord> BasePremiumRules { get; set; } = new List<BasePremiumRuleRecord>();
    public ICollection<DiscountMarkupRuleRecord> DiscountMarkupRules { get; set; } = new List<DiscountMarkupRuleRecord>();
}

internal sealed class BasePremiumRuleRecord
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = string.Empty;
    public string CoverCode { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public string Expression { get; set; } = string.Empty;
    public int Priority { get; set; }
    public TariffRecord? Tariff { get; set; }
}

internal sealed class DiscountMarkupRuleRecord
{
    public int Id { get; set; }
    public string TariffCode { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public decimal Multiplier { get; set; }
    public int Priority { get; set; }
    public TariffRecord? Tariff { get; set; }
}
