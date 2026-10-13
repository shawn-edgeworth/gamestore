using GameStore.Api.Validation;

namespace GameStore.Api.Tests;

public class GameValidatorTests
{
    [Fact]
    public void Validate_ValidGame_ReturnsNull()
    {
        var result = GameValidator.ValidateGame("Aniimo",25.99m);

        Assert.Equal(GameValidationError.None,result);
    }

     [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var result = GameValidator.ValidateGame("", 29.99m);

        Assert.Equal(GameValidationError.NameIsRequired, result);
    }

    [Fact]
    public void Validate_NegativePrice_ReturnsError()
    {
        var result = GameValidator.ValidateGame("Aniimo", -1m);

        Assert.Equal(GameValidationError.PriceCanBeNegative, result);
    }
}
