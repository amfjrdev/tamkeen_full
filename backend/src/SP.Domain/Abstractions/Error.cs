namespace SP.Domain.Abstractions
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None =
            new(string.Empty, string.Empty);

        public static readonly Error NullValue =
            new("Error.NullValue", "Null value");

        // Factory method
        public static Error Validation(string code, string message) =>
            new($"Validation.{code}", message);
    }
}
