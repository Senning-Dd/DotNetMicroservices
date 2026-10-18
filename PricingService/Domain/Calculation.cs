using System;
using System.Collections.Generic;

namespace PricingService.Domain;

public sealed class Calculation
{
    public Calculation(
        string productCode,
        DateTimeOffset policyFrom,
        DateTimeOffset policyTo,
        IReadOnlyCollection<string> selectedCovers,
        IReadOnlyDictionary<string, object> subject)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("A product code is required.", nameof(productCode));
        }

        if (policyTo <= policyFrom)
        {
            throw new ArgumentException("The policy end must be after its start.", nameof(policyTo));
        }

        ProductCode = productCode;
        PolicyFrom = policyFrom;
        PolicyTo = policyTo;
        SelectedCovers = selectedCovers ?? throw new ArgumentNullException(nameof(selectedCovers));
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
    }

    public string ProductCode { get; }
    public DateTimeOffset PolicyFrom { get; }
    public DateTimeOffset PolicyTo { get; }
    public IReadOnlyCollection<string> SelectedCovers { get; }
    public IReadOnlyDictionary<string, object> Subject { get; }
}
