using SP.Domain.Abstractions;

namespace SP.Domain.Shared.Errors;

public class SharedErrors
{
    public static class Email
    {
        public static readonly Error Empty = new("Email.Empty", "Email cannot be null or empty.");
        public static readonly Error InvalidFormat = new("Email.InvalidFormat", "Invalid email format.");
    }

}