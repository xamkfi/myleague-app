using Domain.Entities;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a refresh token for maintaining user sessions
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// The ID of the user this token belongs to
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Navigation property to the user
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// SHA256 hash of the actual refresh token string
        /// </summary>
        public string TokenHash { get; set; }

        /// <summary>
        /// UTC expiration time of the refresh token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// UTC time when the token was revoked, null if still active
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// The ID of the token that replaced this one during rotation
        /// </summary>
        public Guid? ReplacedByTokenId { get; set; }

        /// <summary>
        /// Whether this token has been revoked
        /// </summary>
        public bool IsRevoked => RevokedAt.HasValue;

        /// <summary>
        /// Whether this token has expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Whether this token is still valid (not revoked and not expired)
        /// </summary>
        public bool IsActive => !IsRevoked && !IsExpired;

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected RefreshToken()
        {
            TokenHash = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshToken"/> class.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tokenHash">The SHA256 hash of the token</param>
        /// <param name="expiresAt">The UTC expiration time</param>
        public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException("Token hash cannot be null or empty.", nameof(tokenHash));

            UserId = userId;
            TokenHash = tokenHash;
            ExpiresAt = expiresAt;
        }

        /// <summary>
        /// Revokes this refresh token
        /// </summary>
        /// <param name="replacedByTokenId">Optional ID of the replacement token</param>
        public void Revoke(Guid? replacedByTokenId = null)
        {
            RevokedAt = DateTime.UtcNow;
            ReplacedByTokenId = replacedByTokenId;
        }
    }
}
