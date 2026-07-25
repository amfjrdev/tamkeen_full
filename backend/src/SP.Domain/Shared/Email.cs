using SP.Domain.Shared.Errors;
using SP.Domain.Abstractions;

namespace SP.Domain.Shared;

public record Email(string Value)
{
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(SharedErrors.Email.Empty);
            
        var trimmed = value.Trim().ToLowerInvariant();
        
        if (!IsValidEmail(trimmed))
            return Result.Failure<Email>(SharedErrors.Email.InvalidFormat);
            
        return Result.Success(new Email(trimmed));
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}