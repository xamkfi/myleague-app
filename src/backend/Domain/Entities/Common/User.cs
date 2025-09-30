using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums.Common;

namespace Domain.Entities.Common
{
    /// <summary>
    /// Represents a user
    /// </summary>
    public class User : BaseEntity
    {

        /// <summary>
        /// Username of the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password of the user
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Role of the user
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected User()
        {
            Username = string.Empty;
            PasswordHash = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="passwordHash">The password hash of the user.</param>
        public User(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));

            Username = username;
            PasswordHash = passwordHash;
            Role = UserRole.Admin; // Default role
        }
    }
}
