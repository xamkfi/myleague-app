namespace Domain.Constants;

/// <summary>
/// Identity stored for a team's reusable loan-player placeholders.
/// Sequence 1 is shown as Lainapelaaja #1.
/// </summary>
public static class LoanPlayerNames
{
    public const string FirstName = "Lainapelaaja";

    public static string LastName(int sequence) => $"#{sequence}";

    public static string DisplayName(int sequence) => $"{FirstName} #{sequence}";
}
