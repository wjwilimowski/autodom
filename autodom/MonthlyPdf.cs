// See https://aka.ms/new-console-template for more information

record MonthlyPdf
{
    public required DateTime Month { get; init; }
    public required DateTime Date { get; init; }
    public required string Title { get; init; }
    public required string MId { get; init; }
    public required int Id { get; init; }
    public required decimal Amount { get; init; }
}