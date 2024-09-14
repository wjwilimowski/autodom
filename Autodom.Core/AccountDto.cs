namespace Autodom.Core;

public record AccountDto
{
    public required int User { get; init; }
    public required string Pass { get; init; }
    public required string Email { get; init; }
}