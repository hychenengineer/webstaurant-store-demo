namespace Webstaurant.IDS.Api.Domains.EDI.Models;

public class EdiTransaction
{
    public int Id { get; set; }
    public string TransactionSet { get; set; } = string.Empty; // 850 or 856
    public string ControlNumber { get; set; } = string.Empty;
    public string Direction { get; set; } = "Outbound"; // Outbound or Inbound
    public string PartnerName { get; set; } = string.Empty;
    public string PoNumber { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
