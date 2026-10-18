using System;
using System.Collections.Generic;
using System.Linq;

namespace PricingService.Domain;

public sealed class Tariff
{
    public Tariff(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A tariff code is required.", nameof(code));
        }

        Code = code;
    }

    public string Code { get; }
    public BasePremiumRules BasePremiumRules { get; } = new();
    public DiscountMarkupRules DiscountMarkupRules { get; } = new();

    public TariffPrice CalculatePrice(Calculation calculation)
    {
        ArgumentNullException.ThrowIfNull(calculation);

        if (!string.Equals(Code, calculation.ProductCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The calculation product code does not match this tariff.", nameof(calculation));
        }

        var covers = new Dictionary<string, CoverPrice>(StringComparer.OrdinalIgnoreCase);
        foreach (var coverCode in calculation.SelectedCovers)
        {
            var rule = BasePremiumRules.Rules.FirstOrDefault(candidate =>
                string.Equals(candidate.CoverCode, coverCode, StringComparison.OrdinalIgnoreCase) &&
                (candidate.Condition is null || RuleExpressionEvaluator.EvaluateBoolean(candidate.Condition, calculation.Subject)));

            if (rule is null)
            {
                throw new InvalidOperationException($"No applicable price rule exists for cover '{coverCode}'.");
            }

            var price = RuleExpressionEvaluator.EvaluateDecimal(rule.Expression, calculation.Subject);
            if (price < 0)
            {
                throw new InvalidOperationException($"The calculated price for cover '{coverCode}' cannot be negative.");
            }

            covers[coverCode] = new CoverPrice(price);
        }

        foreach (var markup in DiscountMarkupRules.Rules)
        {
            if (!RuleExpressionEvaluator.EvaluateBoolean(markup.Condition, calculation.Subject))
            {
                continue;
            }

            foreach (var coverCode in covers.Keys.ToArray())
            {
                covers[coverCode] = new CoverPrice(covers[coverCode].Price * markup.Multiplier);
            }
        }

        return new TariffPrice(covers.Values.Sum(cover => cover.Price), covers);
    }
}
