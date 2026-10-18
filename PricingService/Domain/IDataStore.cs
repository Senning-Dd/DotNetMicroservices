using System.Threading;
using System.Threading.Tasks;

namespace PricingService.Domain;

public interface IDataStore
{
    Task<Tariff> GetTariffAsync(string code, CancellationToken cancellationToken = default);
}
