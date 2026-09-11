using Domain.Entities;
using Domain.Enums.Common;
using Domain.ValueObjects.Common;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a system user with passwordless email-based authentication
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Email address used as the login identifier
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// PersonId of the user
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Matching person entity of the user
        /// </summary>
        public Person? Person { get; set; }

        /// <summary>
        /// The role of the user in the system
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// UTC timestamp of the last successful login
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// The pending login code (6-digit), null when no code is active
        /// </summary>
        public string? LoginCode { get; set; }

        /// <summary>
        /// UTC expiration time of the current login code
        /// </summary>
        public DateTime? LoginCodeExpiresAt { get; set; }

        /// <summary>
        /// Number of failed login code verification attempts. Resets on new code request.
        /// </summary>
        public int LoginCodeAttempts { get; set; }

        /// <summary>
        /// Whether the user has verified their email address
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Secure token used for email verification, null when not pending
        /// </summary>
        public string? EmailVerificationToken { get; set; }

        /// <summary>
        /// UTC expiration time of the email verification token
        /// </summary>
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }

        /// <summary>
        /// Navigation property to refresh tokens
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected User()
        {
            Email = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="personId">The person ID linked to this user.</param>
        /// <param name="role">The role of the user (defaults to ClubAdmin).</param>
        public User(string email, Guid personId, UserRole role = UserRole.ClubAdmin)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (personId == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

            Email = EmailAddress.Normalize(email);
            PersonId = personId;
            Role = role;
            IsActive = true;
        }

        /// <summary>
        /// Updates the login email. The address is stored trimmed and lowercased.
        /// </summary>
        public void ChangeEmail(string email)
        {
            Email = EmailAddress.Normalize(email);
        }

        /// <summary>
        /// Sets a new login code with expiration and resets the attempt counter
        /// </summary>
        /// <param name="code">The 6-digit login code</param>
        /// <param name="expiresAt">The UTC expiration time</param>
        public void SetLoginCode(string code, DateTime expiresAt)
        {
            LoginCode = code;
            LoginCodeExpiresAt = expiresAt;
            LoginCodeAttempts = 0;
        }

        /// <summary>
        /// Clears the login code after successful verification
        /// </summary>
        public void ClearLoginCode()
        {
            LoginCode = null;
            LoginCodeExpiresAt = null;
            LoginCodeAttempts = 0;
        }

        /// <summary>
        /// Increments the failed login code attempt counter
        /// </summary>
        public void IncrementLoginCodeAttempts()
        {
            LoginCodeAttempts++;
        }

        /// <summary>
        /// Records a successful login
        /// </summary>
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            ClearLoginCode();
        }

        /// <summary>
        /// Sets a new email verification token with expiration
        /// </summary>
        /// <param name="token">The URL-safe verification token</param>
        /// <param name="expiresAt">The UTC expiration time</param>
        public void SetEmailVerificationToken(string token, DateTime expiresAt)
        {
            EmailVerificationToken = token;
            EmailVerificationTokenExpiresAt = expiresAt;
            IsEmailVerified = false;
        }

        /// <summary>
        /// Completes email verification, activates the account and clears the token
        /// </summary>
        public void VerifyEmail()
        {
            IsEmailVerified = true;
            IsActive = true;
            EmailVerificationToken = null;
            EmailVerificationTokenExpiresAt = null;
        }
    }
}
