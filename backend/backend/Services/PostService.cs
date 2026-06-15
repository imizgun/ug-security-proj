namespace backend.Services;

public class PostService
{
    public const int MaxContentLength = 500;

    public ValidationResult ValidateContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return ValidationResult.Failure("Content cannot be empty.");
        if (content.Trim().Length > MaxContentLength)
            return ValidationResult.Failure($"Content cannot exceed {MaxContentLength} characters.");
        return ValidationResult.Success();
    }

    public bool CanDelete(bool isRequestingUserModerator) => isRequestingUserModerator;
}

public record ValidationResult(bool IsValid, string? Error)
{
    public static ValidationResult Success() => new(true, null);
    public static ValidationResult Failure(string error) => new(false, error);
}
