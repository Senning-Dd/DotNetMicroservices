using DashboardService.Api.Queries.Dtos;

namespace DashboardService.Api.Queries;

public class GetTotalSalesQuery
{
    public string ProductCode { get; set; }
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
}

public class GetAgentsSalesQuery
{
    public DateTime SalesDateFrom { get; set; }
    public DateTime SalesDateTo { get; set; }
}

public class GetSalesTrendsQuery
{
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
