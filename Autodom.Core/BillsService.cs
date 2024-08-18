namespace Autodom.Core;

public class LastProcessedBillsRecord
{
    public int User { get; set; }
    public DateTime DateTime { get; set; }
    public HashSet<int> ProcessedBillIds { get; set; } = new();
}

public class BillsService
{
    public IEnumerable<BillDto> FindUnprocessedBills(IEnumerable<BillDto> bills, LastProcessedBillsRecord record)
    {
        yield break;
    } 
}