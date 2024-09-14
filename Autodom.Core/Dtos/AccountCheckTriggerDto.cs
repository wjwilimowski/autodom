namespace Autodom.Core.Dtos;

public record AccountCheckTriggerDto
{
    public required int User { get; init; }
    public required string Pass { get; init; }
    public required string Email { get; init; }
}