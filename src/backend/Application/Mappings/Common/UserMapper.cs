using System;
using System.Collections.Generic;
using System.Linq;
using Application.Commands.Users;
using Application.DTOs.Common;
using Application.Mappings.Common;
using Domain.Entities.Common;

namespace Application.Mappings.Common
{
    /// <summary>
    /// Mapper class for User entity and related DTOs
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Maps a User to a UserDto
        /// </summary>
        /// <param name="user">The user entity</param>
        /// <returns>The mapped UserDto</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static UserDto ToDto(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserDto(
                user.Id,
                user.Username,
                user.Role
            );
        }

        /// <summary>
        /// Maps a collection of User entities to a collection of UserDtos
        /// </summary>
        /// <param name="users">The user entities</param>
        /// <returns>The mapped UserDtos</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<UserDto> ToDtos(IEnumerable<User> users)
        {
            if (users == null)
                throw new ArgumentNullException(nameof(users));

            return users.Select(user => ToDto(user));
        }

        /// <summary>
        /// Maps a CreateUserCommand to a User entity
        /// </summary>
        /// <param name="command">The create command</param>
        /// <returns>The new User entity</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static User ToEntity(CreateUserCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            string hashedPassword = HashPassword(command.Password);
            
            return new User(
                command.Username,
                hashedPassword
            );
        }

        /// <summary>
        /// Updates a User entity from an UpdateUserCommand
        /// </summary>
        /// <param name="user">The user entity to update</param>
        /// <param name="command">The update command</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void UpdateFromCommand(User user, UpdateUserCommand command)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            user.Username = command.Username;
            
            if (!string.IsNullOrEmpty(command.Password))
            {
                user.PasswordHash = HashPassword(command.Password);
            }
        }

        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hash">The hash to verify against</param>
        /// <returns>True if the password matches the hash</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
} 
