namespace Autodom.Core;

public class LastProcessedBillsRecord
{
    public int User { get; set; }
    public DateTime DateTime { get; set; }
    public HashSet<int> ProcessedBillIds { get; set; } = new();
}

public class BillsService
{
    public (List<BillDto> billsToProcess, LastProcessedBillsRecord recordToSave) FindUnprocessedBills(IEnumerable<BillDto> bills, LastProcessedBillsRecord record)
    {
        return ([], record);
    } 
}