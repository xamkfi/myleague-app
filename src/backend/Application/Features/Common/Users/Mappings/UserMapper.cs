using Application.Features.Common.Users.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;

namespace Application.Features.Common.Users.Mappings;

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
    public static UserDto ToDto(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto(
            user.Id,
            user.Email,
            user.PersonId,
            user.Role,
            user.IsActive,
            user.IsEmailVerified,
            user.LastLoginAt,
            user.Person != null ? PersonMapper.ToDto(user.Person) :
                new PersonDto(user.PersonId, "Unknown", "User", DateTime.MinValue, "Unknown User",
                    Domain.Enums.Common.PersonRole.User, false, null, null)
        );
    }

    /// <summary>
    /// Maps a collection of User entities to a collection of UserDtos
    /// </summary>
    /// <param name="users">The user entities</param>
    /// <returns>The mapped UserDtos</returns>
    public static IEnumerable<UserDto> ToDtos(IEnumerable<User> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        return users.Select(ToDto);
    }

    /// <summary>
    /// Maps a CreateUserCommand to a User entity
    /// </summary>
    /// <param name="command">The create command</param>
    /// <returns>The new User entity</returns>
    public static User ToEntity(CreateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return new User(command.Email, command.PersonId, command.Role);
    }

    /// <summary>
    /// Updates a User entity from an UpdateUserCommand
    /// </summary>
    /// <param name="user">The user entity to update</param>
    /// <param name="command">The update command</param>
    public static void UpdateFromCommand(User user, UpdateUserCommand command)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(command);

        user.Email = command.Email;
        user.Role = command.Role;
    }
}
