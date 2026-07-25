namespace SP.Application.Abstractions.Authentication;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AllowAnonymousAttribute : Attribute
{
}

public interface IAllowAnonymous
{
}
