using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PricingService.Domain;

namespace PricingService.Persistence;

public sealed class PostgresDataStore : IDataStore
{
    private readonly PricingDbContext dbContext;

    public PostgresDataStore(PricingDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Tariff> GetTariffAsync(string code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Tariffs
            .AsNoTracking()
            .Include(tariff => tariff.BasePremiumRules)
            .Include(tariff => tariff.DiscountMarkupRules)
            .SingleOrDefaultAsync(tariff => tariff.Code == code, cancellationToken);

        if (record is null)
        {
            throw new KeyNotFoundException($"No tariff was found for product code '{code}'.");
        }

        var tariff = new Tariff(record.Code);
        foreach (var rule in record.BasePremiumRules.OrderBy(rule => rule.Priority))
        {
            tariff.BasePremiumRules.AddBasePriceRule(rule.CoverCode, rule.Condition, rule.Expression);
        }

        foreach (var rule in record.DiscountMarkupRules.OrderBy(rule => rule.Priority))
        {
            tariff.DiscountMarkupRules.AddPercentMarkup(rule.Condition, rule.Multiplier);
        }

        return tariff;
    }
}
