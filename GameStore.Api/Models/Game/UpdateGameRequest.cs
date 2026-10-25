namespace GameStore.Api.Models;

public class UpdateGameRequest
{
    public string Name {get; set;} = string.Empty;
    public decimal Price {get; set;}
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}