namespace Autodom.Core;

public record AccountConfig
{
    public required int User { get; init; }
    public required string Pass { get; init; }
    public required List<string> EmailsToNotify { get; init; } = [];
}