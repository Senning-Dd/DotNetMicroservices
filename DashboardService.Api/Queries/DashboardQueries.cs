using DashboardService.Api.Queries.Dtos;
using MediatR;

namespace DashboardService.Api.Queries;

public class GetTotalSalesQuery : IRequest<GetTotalSalesResult>
{
    public string ProductCode { get; set; }
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
}

public class GetAgentsSalesQuery : IRequest<GetAgentsSalesResult>
{
    public string AgentLogin { get; set; }
    public string ProductCode { get; set; }
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
}

public class GetSalesTrendsQuery : IRequest<GetSalesTrendsResult>
{
    public string ProductCode { get; set; }
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
    public TimeUnit Unit { get; set; }
}

public enum TimeUnit
{
    Day,
    Week,
    Month,
    Year
}

public class GetTotalSalesResult
{
    public SalesTotalDto Total { get; set; }
    public Dictionary<string, SalesTotalDto> PerProductTotal { get; set; } = new();
}

public class GetAgentsSalesResult
{
    public Dictionary<string, SalesTotalDto> PerAgentTotal { get; set; } = new();
}

public class GetSalesTrendsResult
{
    public List<PeriodSalesDto> PeriodsSales { get; set; } = new();
}
