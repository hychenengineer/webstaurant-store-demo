using Microsoft.AspNetCore.Mvc;
using Webstaurant.IDS.Api.Domains.Purchasing.Services;

namespace Webstaurant.IDS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasingController : ControllerBase
{
    private readonly IPurchasingService _purchasingService;

    public PurchasingController(IPurchasingService purchasingService)
    {
        _purchasingService = purchasingService;
    }

    [HttpGet("pos")]
    public async Task<IActionResult> GetAllPos()
    {
        return Ok(await _purchasingService.GetAllPosAsync());
    }

    [HttpPost("simulate-asn")]
    public async Task<IActionResult> SimulateAsn([FromBody] SimulateAsnRequest req)
    {
        var raw856 = req.RawPayload;
        if (string.IsNullOrWhiteSpace(raw856))
        {
            var dateStr = DateTime.UtcNow.ToString("yyMMdd");
            var tracking = "1Z" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper();
            raw856 = $"ISA*00*          *00*          *01*VENDOR         *ZZ*WEBSTAURANT   *{dateStr}*1200*U*00401*000000001*0*P*>~\n" +
                     $"GS*SH*VENDOR*WEBSTAURANT*{dateStr}*1200*1*X*004010~\n" +
                     $"ST*856*0001~\n" +
                     $"BSN*00*ASN-{DateTime.UtcNow.Ticks % 1000000}*{dateStr}*1200~\n" +
                     $"HL*1**S~\n" +
                     $"PRF*{req.PoNumber}~\n" +
                     $"REF*2I*{tracking}~\n" +
                     $"HL*2*1*O~\n" +
                     $"HL*3*2*I~\n" +
                     $"CTT*1~\n" +
                     $"SE*10*0001~\n" +
                     $"GE*1*1~\n" +
                     $"IEA*1*000000001~";
        }

        var po = await _purchasingService.ProcessVendorAsnAsync(raw856);
        return Ok(po);
    }
}

public class SimulateAsnRequest
{
    public string PoNumber { get; set; } = string.Empty;
    public string? RawPayload { get; set; }
}
