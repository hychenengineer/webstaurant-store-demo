using Microsoft.AspNetCore.Mvc;
using Webstaurant.IDS.Api.Domains.EDI.Services;

namespace Webstaurant.IDS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EdiController : ControllerBase
{
    private readonly IEdiService _ediService;

    public EdiController(IEdiService ediService)
    {
        _ediService = ediService;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        return Ok(await _ediService.GetTransactionsAsync());
    }
}
