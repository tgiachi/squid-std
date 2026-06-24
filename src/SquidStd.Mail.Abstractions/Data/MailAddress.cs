namespace SquidStd.Mail.Abstractions.Data;

/// <summary>An email address with an optional display name.</summary>
/// <param name="Name">Display name (may be empty).</param>
/// <param name="Address">The email address.</param>
public sealed record MailAddress(string Name, string Address);
