namespace Autodom.Core.Dtos;

public record AccountCheckTriggerDto
{
    public int Id { get; set; }
    public required int User { get; init; }
    public required string Pass { get; init; }
    public required string Email { get; init; }
    public required string ApartmentName { get; init; }
}