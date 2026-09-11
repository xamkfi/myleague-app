namespace Application.Features.Common.Deletion;

/// <summary>
/// Stable English messages returned when a hard delete is blocked.
/// The frontend maps these strings to i18n keys.
/// </summary>
public static class DeletionReasons
{
    public const string PersonHasMatchRecords =
        "Cannot delete this person because they have match or statistics records.";

    public const string PersonHasUserAccount =
        "Cannot delete this person because they have a user account.";

    public const string PersonIsClubManager =
        "Cannot delete this person because they are a club administrator.";

    public const string PersonIsAssignedOfficial =
        "Cannot delete this person because they are assigned as an official on a match.";

    public const string PlayerHasHistory =
        "Cannot delete this player because they have played in a match or have statistics.";

    public const string TeamUsedInMatches =
        "Cannot delete this team because it is used in matches.";

    public const string ClubHasTeams =
        "Cannot delete this club because it still has teams.";

    public const string DivisionHasTeams =
        "Cannot delete this division because teams still use it. Deactivate it instead.";

    public const string RefereeAssignedToMatch =
        "Cannot delete this referee because they are assigned to a match.";

    public const string LastSystemAdmin =
        "Cannot delete the last system administrator.";

    public const string CannotDeleteOwnAccount =
        "You cannot delete your own user account.";
}
