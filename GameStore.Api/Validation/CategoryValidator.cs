namespace GameStore.Api.Validation;

public enum CategoryValidationError
{
    None = 0,
    NameIsRequired = 1,
}

public static class CategoryValidator
{
    public static CategoryValidationError ValidateCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return CategoryValidationError.NameIsRequired;
        }

        return CategoryValidationError.None;
    }

    public static string ToMessage(this CategoryValidationError error)
    {
        switch (error)
        {
            case CategoryValidationError.None:
                return string.Empty;
            case CategoryValidationError.NameIsRequired:
                return "Category name is required.";
            default:
                return "Unhandeled error.";
        }
    }
}