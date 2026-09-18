namespace Metaspesa.RestApi.Identity;

/// <summary>Credentials supplied when registering or authenticating a shopper.</summary>
/// <param name="Username">Shopper username used to identify the account.</param>
/// <param name="Password">Plaintext password for this request; never returned in responses.</param>
internal sealed record CredentialsRequest(string Username, string Password);