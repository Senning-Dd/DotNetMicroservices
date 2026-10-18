using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using PricingService.Api.Commands;
using PricingService.Domain;

namespace PricingService.Commands;

public sealed class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore dataStore;

    public CalculatePriceHandler(IDataStore dataStore)
    {
        this.dataStore = dataStore;
    }

    public async Task<CalculatePriceResult> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        new CalculatePriceCommandValidator().ValidateAndThrow(request);

        var tariff = await dataStore.GetTariffAsync(request.ProductCode, cancellationToken);
        var subject = request.Answers.ToDictionary(
            answer => answer.QuestionCode,
            answer => answer.GetAnswer(),
            System.StringComparer.OrdinalIgnoreCase);
        var calculation = new Calculation(
            request.ProductCode,
            request.PolicyFrom,
            request.PolicyTo,
            request.SelectedCovers,
            subject);
        var price = tariff.CalculatePrice(calculation);

        return new CalculatePriceResult
        {
            TotalPrice = price.TotalPremium,
            CoverPrices = price.Covers.ToDictionary(cover => cover.Key, cover => cover.Value.Price)
        };
    }
}
