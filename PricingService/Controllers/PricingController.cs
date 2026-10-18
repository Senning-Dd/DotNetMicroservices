using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PricingService.Api.Commands;

namespace PricingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PricingController : ControllerBase
{
    private readonly IMediator mediator;

    public PricingController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post(CalculatePriceCommand command, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return new JsonResult(result);
    }
}
