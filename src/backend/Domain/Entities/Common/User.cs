using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

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
        /// PersonId of the user
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Matching person entity of the user
        /// </summary>
        public Person Person { get; set; }

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
        /// <param name="personId">The person ID linked to this user.</param>
        public User(string username, string passwordHash, Guid personId)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));
            if (personId == Guid.Empty)
                throw new ArgumentException("Person ID cannot be empty.", nameof(personId));

            Username = username;
            PasswordHash = passwordHash;
            PersonId = personId;
        }
    }
}
