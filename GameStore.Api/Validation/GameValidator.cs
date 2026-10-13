namespace GameStore.Api.Validation;

public enum GameValidationError
{
    None = 0,
    NameIsRequired = 1,
    PriceIsRequired = 2,
    PriceCanBeNegative = 3,
    CategoryIsRequired = 4
}

public static class GameValidator
{
    public static GameValidationError ValidateGame(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GameValidationError.NameIsRequired;
        }

        if (price < 0)
        {
            return GameValidationError.PriceCanBeNegative;
        }

        return GameValidationError.None;
    }

    public static string ToMessage(this GameValidationError error)
    {
        switch (error)
        {
            case GameValidationError.None:
                return string.Empty;
            case GameValidationError.NameIsRequired:
                return "Game name is required.";
            case GameValidationError.PriceIsRequired:
                return "Game price is required.";
            case GameValidationError.PriceCanBeNegative:
                return "Price can't be negative.";
            case GameValidationError.CategoryIsRequired:
                return "Game category is required.";
            default:
                return "Unhandeled error.";
        }
    }
}