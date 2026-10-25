using MediatR;
using PolicyService.Api.Commands.Dtos;

namespace PolicyService.Api.Events;

public class PolicyCreated : INotification
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime PolicyFrom { get; set; }
    public DateTime PolicyTo { get; set; }
    public PersonDto PolicyHolder { get; set; } = new();
    public decimal TotalPremium { get; set; }
    public string AgentLogin { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
}
