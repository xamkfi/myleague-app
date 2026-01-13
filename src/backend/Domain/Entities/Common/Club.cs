using Domain.Entities.Floorball;
using Domain.Entities.Hockey;
using Domain.Enums.Common;
using Domain.Enums.Hockey;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a sports club that can have both floorball and hockey teams.
    /// </summary>
    public class Club : BaseEntity
    {
        /// <summary>
        /// Gets the name of the club.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the founding date of the club.
        /// </summary>
        public DateTime FoundingDate { get; private set; }

        /// <summary>
        /// Gets the city where the club is located.
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// Gets the country where the club is located.
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Gets the website URL of the club.
        /// </summary>
        public Uri WebsiteUrl { get; private set; }

        /// <summary>
        /// Gets the logo URL of the club.
        /// </summary>
        public Uri LogoUrl { get; private set; }

        /// <summary>
        /// Gets the contact email address of the club.
        /// </summary>
        public string ContactEmail { get; private set; }

        private readonly List<FloorballTeam> _floorballTeams = new();
        private readonly List<HockeyTeam> _hockeyTeams = new();

        /// <summary>
        /// Gets the list of floorball teams associated with the club.
        /// </summary>
        public IReadOnlyList<FloorballTeam> FloorballTeams => _floorballTeams;

        /// <summary>
        /// Gets the list of hockey teams associated with the club.
        /// </summary>
        public IReadOnlyList<HockeyTeam> HockeyTeams => _hockeyTeams;

        /// <summary>
        /// Private constructor for ORM and serialization.
        /// </summary>
        private Club()
        {
            Name = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            WebsiteUrl = new Uri("https://example.com");
            LogoUrl = new Uri("https://example.com/logo.png");
            ContactEmail = "contact@example.com";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Club"/> class.
        /// </summary>
        /// <param name="name">The name of the club.</param>
        /// <param name="city">The city where the club is located (optional).</param>
        /// <param name="country">The country where the club is located (optional).</param>
        /// <param name="foundingDate">The founding date of the club (optional).</param>
        /// <param name="websiteUrl">The website URL of the club (optional).</param>
        /// <param name="logoUrl">The logo URL of the club (optional).</param>
        /// <param name="contactEmail">The contact email address of the club (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if required parameters are empty or whitespace.</exception>
        public Club(
            string name,
            string? city = null,
            string? country = null,
            DateTime? foundingDate = null,
            Uri? websiteUrl = null,
            Uri? logoUrl = null,
            string? contactEmail = null)
        {
            ValidateRequired(name, nameof(name));

            Id = Guid.NewGuid();
            Name = name;
            City = city ?? string.Empty;
            Country = country ?? string.Empty;
            FoundingDate = foundingDate ?? DateTime.UtcNow;
            WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
            LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
            ContactEmail = contactEmail ?? "contact@example.com";

        }

        /// <summary>
        /// Updates the basic information of the club.
        /// </summary>
        /// <param name="name">The new name of the club.</param>
        /// <param name="city">The new city of the club (optional).</param>
        /// <param name="country">The new country of the club (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if required parameters are empty or whitespace.</exception>
        public void UpdateBasicInfo(string name, string? city = null, string? country = null)
        {
            ValidateRequired(name, nameof(name));

            Name = name;
            City = city ?? string.Empty;
            Country = country ?? string.Empty;

        }

        /// <summary>
        /// Updates the online presence information of the club.
        /// </summary>
        /// <param name="websiteUrl">The new website URL (optional).</param>
        /// <param name="logoUrl">The new logo URL (optional).</param>
        /// <param name="contactEmail">The new contact email address (optional).</param>
        public void UpdateOnlinePresence(Uri? websiteUrl, Uri? logoUrl, string? contactEmail)
        {
            WebsiteUrl = websiteUrl ?? new Uri("https://example.com");
            LogoUrl = logoUrl ?? new Uri("https://example.com/logo.png");
            ContactEmail = contactEmail ?? "contact@example.com";
        }

        /// <summary>
        /// Updates the founding date of the club.
        /// </summary>
        /// <param name="foundingDate">The new founding date of the club (optional).</param>
        public void UpdateFoundingDate(DateTime? foundingDate)
        {
            if (foundingDate.HasValue)
            {
                FoundingDate = foundingDate.Value;
            }
        }

        /// <summary>
        /// Adds a new floorball team to the club.
        /// </summary>
        /// <param name="name">The name of the team.</param>
        /// <param name="divisionId">The division of the team.</param>
        /// <param name="homeArena">The home arena of the team.</param>
        /// <param name="primaryJerseyColor">The primary jersey color of the team.</param>
        /// <param name="teamCategory">The category of the team (Adult, Youth, Women).</param>
        /// <param name="secondaryColor">The secondary jersey color of the team (optional).</param>
        /// <returns>The created <see cref="FloorballTeam"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if required parameters are empty or whitespace.</exception>
        public FloorballTeam AddFloorballTeam(string name, Guid divisionId, string homeArena, string primaryJerseyColor, TeamCategory teamCategory, string? secondaryColor = null)
        {
            ValidateRequired(name, nameof(name));
            ValidateRequired(homeArena, nameof(homeArena));
            ValidateRequired(primaryJerseyColor, nameof(primaryJerseyColor));

            var team = new FloorballTeam(name, divisionId, this, homeArena, primaryJerseyColor, teamCategory, secondaryColor);
            _floorballTeams.Add(team);
            return team;
        }

        /// <summary>
        /// Removes a floorball team from the club by its identifier.
        /// </summary>
        /// <param name="teamId">The unique identifier of the team to remove.</param>
        /// <returns><c>true</c> if the team was removed; otherwise, <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the team has active members.</exception>
        public bool RemoveFloorballTeam(Guid teamId)
        {
            FloorballTeam? team = _floorballTeams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return false;

            if (team.HasActiveMembers)
                throw new InvalidOperationException($"Cannot remove team {team.Name} as it has active members.");

            _floorballTeams.Remove(team);
            return true;
        }

        /// <summary>
        /// Gets all floorball teams in the specified division.
        /// </summary>
        /// <param name="division">The division to filter by.</param>
        /// <returns>An enumerable of <see cref="FloorballTeam"/> in the specified division.</returns>
        public IEnumerable<FloorballTeam> GetFloorballTeamsByDivision(Division division) =>
            _floorballTeams.Where(t => t.Division == division);

        /// <summary>
        /// Adds a new hockey team to the club.
        /// </summary>
        /// <param name="name">The name of the team.</param>
        /// <param name="division">The division of the team.</param>
        /// <param name="homeArena">The home arena of the team.</param>
        /// <param name="primaryJerseyColor">The primary jersey color of the team.</param>
        /// <param name="secondaryColor">The secondary jersey color of the team (optional).</param>
        /// <returns>The created <see cref="HockeyTeam"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown if required parameters are empty or whitespace.</exception>
        public HockeyTeam AddHockeyTeam(string name, HockeyDivision division, string homeArena, string primaryJerseyColor, string? secondaryColor = null)
        {
            ValidateRequired(name, nameof(name));
            ValidateRequired(homeArena, nameof(homeArena));
            ValidateRequired(primaryJerseyColor, nameof(primaryJerseyColor));

            HockeyTeam team = new HockeyTeam(name, division, this, homeArena, primaryJerseyColor, secondaryColor);
            _hockeyTeams.Add(team);
            return team;
        }

        /// <summary>
        /// Removes a hockey team from the club by its identifier.
        /// </summary>
        /// <param name="teamId">The unique identifier of the team to remove.</param>
        /// <returns><c>true</c> if the team was removed; otherwise, <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the team has active members.</exception>
        public bool RemoveHockeyTeam(Guid teamId)
        {
            HockeyTeam? team = _hockeyTeams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return false;

            if (team.HasActiveMembers)
                throw new InvalidOperationException($"Cannot remove team {team.Name} as it has active members.");

            _hockeyTeams.Remove(team);
            return true;
        }

        /// <summary>
        /// Gets all hockey teams in the specified division.
        /// </summary>
        /// <param name="division">The division to filter by.</param>
        /// <returns>An enumerable of <see cref="HockeyTeam"/> in the specified division.</returns>
        public IEnumerable<HockeyTeam> GetHockeyTeamsByDivision(HockeyDivision division) =>
            _hockeyTeams.Where(t => t.Division == division);

        /// <summary>
        /// Validates that a required string parameter is not null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the value is empty or whitespace.</exception>
        private static void ValidateRequired(string? value, string paramName)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }
    }
}
