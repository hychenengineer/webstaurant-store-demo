using System.Text;
using Webstaurant.IDS.Api.Domains.EDI.Models;
using Webstaurant.IDS.Api.Domains.Purchasing.Models;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.EDI.Services;

public interface IEdiService
{
    Task<string> Generate850PurchaseOrderAsync(VendorPurchaseOrder po, string? shipToName = null, string? shipToAddress = null, string? shipToCity = null, string? shipToState = null, string? shipToZip = null);
    (string poNumber, string trackingNumber, DateTime shipDate) Parse856AdvanceShipNotice(string rawX12);
    Task<IEnumerable<EdiTransaction>> GetTransactionsAsync();
}

public class EdiService : IEdiService
{
    private readonly IEdiRepository _ediRepo;

    public EdiService(IEdiRepository ediRepo)
    {
        _ediRepo = ediRepo;
    }

    public async Task<string> Generate850PurchaseOrderAsync(
        VendorPurchaseOrder po, 
        string? shipToName = null, 
        string? shipToAddress = null, 
        string? shipToCity = null, 
        string? shipToState = null, 
        string? shipToZip = null)
    {
        var controlNum = $"{po.Id:D9}";
        var dateStr = DateTime.UtcNow.ToString("yyMMdd");
        var timeStr = DateTime.UtcNow.ToString("HHmm");
        var sb = new StringBuilder();

        var vendorCode = po.VendorName.Length > 15 ? po.VendorName.Substring(0, 15) : po.VendorName.PadRight(15);

        // ISA - Interchange Control Header
        sb.Append($"ISA*00*          *00*          *ZZ*WEBSTAURANT   *01*{vendorCode}*{dateStr}*{timeStr}*U*00401*{controlNum}*0*P*>~\n");
        // GS - Functional Group Header
        sb.Append($"GS*PO*WEBSTAURANT*{po.VendorName}*{dateStr}*{timeStr}*1*X*004010~\n");
        // ST - Transaction Set Header (850 = Purchase Order)
        sb.Append($"ST*850*{controlNum.Substring(5)}~\n");
        // BEG - Beginning Segment for Purchase Order
        sb.Append($"BEG*00*SA*{po.PoNumber}**{dateStr}~\n");
        // CUR - Currency Reference
        sb.Append("CUR*BY*USD~\n");
        // REF - Reference Identification (Order Type)
        sb.Append($"REF*TN*{po.PoType.ToUpper()}~\n");

        // N1/N3/N4 - Name & Address (Ship To)
        if (!string.IsNullOrEmpty(shipToName))
        {
            sb.Append($"N1*ST*{shipToName}~\n");
            sb.Append($"N3*{shipToAddress ?? "100 Customer Way"}~\n");
            sb.Append($"N4*{shipToCity ?? "Lititz"}*{shipToState ?? "PA"}*{shipToZip ?? "17543"}*US~\n");
        }
        else
        {
            sb.Append("N1*ST*WEBSTAURANT LITITZ DC~\n");
            sb.Append("N3*1000 Commercial Way~\n");
            sb.Append("N4*Lititz*PA*17543*US~\n");
        }

        // PO1 - Baseline Item Data
        int line = 1;
        foreach (var item in po.Items)
        {
            sb.Append($"PO1*{line:D3}*{item.Quantity}*EA*{item.UnitPrice:F2}*PE*VN*{item.Sku}*IN*{item.Sku}~\n");
            sb.Append($"PID*F****{item.ProductName}~\n");
            line++;
        }

        // CTT - Transaction Totals
        sb.Append($"CTT*{po.Items.Count}~\n");
        // SE - Transaction Set Trailer
        sb.Append($"SE*{7 + (po.Items.Count * 2)}*{controlNum.Substring(5)}~\n");
        // GE - Functional Group Trailer
        sb.Append("GE*1*1~\n");
        // IEA - Interchange Control Trailer
        sb.Append($"IEA*1*{controlNum}~");

        var rawEdi = sb.ToString();

        await _ediRepo.SaveTransactionAsync(new EdiTransaction
        {
            TransactionSet = "850",
            ControlNumber = controlNum,
            Direction = "Outbound",
            PartnerName = po.VendorName,
            PoNumber = po.PoNumber,
            RawPayload = rawEdi,
            CreatedAt = DateTime.UtcNow
        });

        return rawEdi;
    }

    public (string poNumber, string trackingNumber, DateTime shipDate) Parse856AdvanceShipNotice(string rawX12)
    {
        string poNumber = "";
        string trackingNumber = "";
        DateTime shipDate = DateTime.UtcNow;

        var segments = rawX12.Split(new[] { '~', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var seg in segments)
        {
            var parts = seg.Trim().Split('*');
            if (parts.Length < 2) continue;

            if (parts[0] == "PRF" && parts.Length >= 2)
            {
                poNumber = parts[1];
            }
            if (parts[0] == "BSN" && parts.Length >= 4)
            {
                if (DateTime.TryParseExact(parts[3], "yyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    shipDate = parsedDate;
                }
            }
            if ((parts[0] == "REF" || parts[0] == "MAN") && parts.Length >= 3)
            {
                trackingNumber = parts[2];
            }
        }

        if (string.IsNullOrEmpty(trackingNumber))
        {
            trackingNumber = "1Z" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper();
        }

        return (poNumber, trackingNumber, shipDate);
    }

    public async Task<IEnumerable<EdiTransaction>> GetTransactionsAsync()
    {
        return await _ediRepo.GetAllTransactionsAsync();
    }
}
