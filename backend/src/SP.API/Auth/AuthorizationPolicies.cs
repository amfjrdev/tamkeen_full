namespace SP.API.Auth;

/// <summary>
/// Central place for authorization policy and role names used in the API.
/// </summary>
public static class AuthorizationPolicies
{
    // Role names
    public const string AdminRole = "Admin";
    public const string ProviderRole = "Provider";
    public const string ClientRole = "Client";

    // Policy names
    public const string AdminOnly = "AdminOnly";
    public const string ProviderOnly = "ProviderOnly";
    public const string ClientOnly = "ClientOnly";
}

