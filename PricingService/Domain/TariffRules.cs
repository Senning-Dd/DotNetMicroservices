using System;
using System.Collections.Generic;

namespace PricingService.Domain;

public sealed class BasePremiumRules
{
    private readonly List<BasePriceRule> rules = new();

    public IReadOnlyList<BasePriceRule> Rules => rules;

    public void AddBasePriceRule(string coverCode, string? condition, string expression)
    {
        if (string.IsNullOrWhiteSpace(coverCode))
        {
            throw new ArgumentException("A cover code is required.", nameof(coverCode));
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("A price expression is required.", nameof(expression));
        }

        rules.Add(new BasePriceRule(coverCode, condition, expression));
    }
}

public sealed class DiscountMarkupRules
{
    private readonly List<PercentMarkupRule> rules = new();

    public IReadOnlyList<PercentMarkupRule> Rules => rules;

    public void AddPercentMarkup(string condition, decimal multiplier)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("A markup condition is required.", nameof(condition));
        }

        if (multiplier < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "A markup multiplier cannot be negative.");
        }

        rules.Add(new PercentMarkupRule(condition, multiplier));
    }
}

public sealed record BasePriceRule(string CoverCode, string? Condition, string Expression);

public sealed record PercentMarkupRule(string Condition, decimal Multiplier);
