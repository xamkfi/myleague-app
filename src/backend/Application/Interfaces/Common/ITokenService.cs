using Domain.Entities.Common;

namespace Application.Interfaces.Common
{
    /// <summary>
    /// Abstraction for generating JWT tokens for users
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="user">The user to generate a token for</param>
        /// <returns>Signed JWT token string</returns>
        string GenerateToken(User user);
    }
}


