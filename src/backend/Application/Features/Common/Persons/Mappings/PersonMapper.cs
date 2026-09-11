using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Features.Common.Persons.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Persons.DTOs;
using Application.Features.Common.Clubs.DTOs;
using Application.Features.Common.Divisions.DTOs;
using Application.Features.Common.News.DTOs;
using Application.Features.Common.Search.DTOs;
using Application.Features.Common.MatchTimer.DTOs;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;
using Domain.ValueObjects.Common;

namespace Application.Features.Common.Persons.Mappings
{
    /// <summary>
    /// Mapper class for Person entity and related DTOs
    /// </summary>
    public static class PersonMapper
    {
        /// <summary>
        /// Maps a Person to a PersonDto
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static PersonDto ToDto(Person person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            return new PersonDto(
                person.Id,
                person.FirstName,
                person.LastName,
                person.BirthDate,
                person.FullName,
                person.role,
                person.IsRegistered,
                person.Address,
                person.ContactInfo
            );
        }

        public static PersonPublicDto ToPublicDto(Person person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            return new PersonPublicDto(
                person.Id,
                person.FirstName,
                person.LastName,
                person.BirthDate,
                person.FullName,
                person.IsRegistered
            );
        }

        /// <summary>
        /// Maps a collection of Person entities to a collection of PersonDtos
        /// </summary>
        /// <param name="persons"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<PersonDto> ToDtos(IEnumerable<Person> persons)
        {
            if(persons == null)
            {
                throw new ArgumentNullException(nameof(persons));
            }
            return persons.Select(person => ToDto(person));
        }

        /// <summary>
        /// Updates a Person entity from an UpdatePersonCommand
        /// </summary>
        /// <param name="person"></param>
        /// <param name="command"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void UpdateFromCommand(Person person, UpdatePersonCommand command)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            // Ensure DateTime is in UTC if BirthDate is provided
            DateTime? birthDateUtc = null;
            if (command.BirthDate.HasValue)
            {
                birthDateUtc = command.BirthDate.Value.Kind switch
                {
                    DateTimeKind.Utc => command.BirthDate.Value,
                    DateTimeKind.Local => command.BirthDate.Value.ToUniversalTime(),
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(command.BirthDate.Value, DateTimeKind.Utc),
                    _ => DateTime.SpecifyKind(command.BirthDate.Value, DateTimeKind.Utc)
                };
            }

            person.UpdateBasicInfo(command.FirstName, command.LastName);
            person.UpdateAddress(ToAddress(command.Address));
            person.UpdateContactInfo(ToContactInfo(command.ContactInfo));
            person.UpdateBirthDate(birthDateUtc);
            person.UpdateIsRegistered(command.IsRegistered);
        }


        /// <summary>
        /// Maps an AddressDto to an Address value object.
        ///
        /// Returns <c>null</c> when the DTO has no actionable data (every part is blank). EF Core
        /// stores the owned-type Address as all-NULL columns in that case, mirroring the same
        /// "empty block → no row" convention used by <c>ToContactInfo</c>. This keeps tournament
        /// imports that never carry address info from triggering "Country is required" errors.
        /// </summary>
        public static Address? ToAddress(AddressDto? dto)
        {
            if (dto == null) return null;

            bool hasAny =
                !string.IsNullOrWhiteSpace(dto.Street1) ||
                !string.IsNullOrWhiteSpace(dto.Street2) ||
                !string.IsNullOrWhiteSpace(dto.City) ||
                !string.IsNullOrWhiteSpace(dto.PostalCode) ||
                !string.IsNullOrWhiteSpace(dto.Country);
            if (!hasAny)
            {
                return null;
            }

            return new Address(
                dto.Street1,
                dto.City,
                dto.PostalCode,
                dto.Country,
                dto.Street2
            );
        }

        /// <summary>
        /// Maps an Address value object to an AddressDto
        /// </summary>
        public static AddressDto? ToAddressDto(Address? address)
        {
            if (address == null) return null;
            return new AddressDto(
                address.Street1,
                address.Street2 ?? string.Empty,
                address.City,
                address.PostalCode,
                address.Country
            );
        }

        /// <summary>
        /// Maps a ContactInfoDto to a ContactInfo value object.
        ///
        /// Returns <c>null</c> when the DTO has no actionable data (no email AND no phones).
        /// EF Core treats a null owned-type instance as "all columns NULL", which keeps the DB
        /// row tidy for tournament-imported players that don't carry contact details at all.
        /// </summary>
        public static ContactInfo? ToContactInfo(ContactInfoDto? dto)
        {
            if (dto == null) return null;

            bool hasEmail = !string.IsNullOrWhiteSpace(dto.Email);
            bool hasPhone = !string.IsNullOrWhiteSpace(dto.Phone);
            bool hasAltPhone = !string.IsNullOrWhiteSpace(dto.AlternativePhone);
            if (!hasEmail && !hasPhone && !hasAltPhone)
            {
                return null;
            }

            return new ContactInfo(
                hasEmail ? dto.Email : null,
                hasPhone ? dto.Phone : null,
                hasAltPhone ? dto.AlternativePhone : null
            );
        }

        /// <summary>
        /// Maps a ContactInfo value object to a ContactInfoDto
        /// </summary>
        public static ContactInfoDto? ToContactInfoDto(ContactInfo? contactInfo)
        {
            if (contactInfo == null) return null;
            return new ContactInfoDto(
                contactInfo.Email,
                contactInfo.Phone ?? string.Empty,
                contactInfo.AlternativePhone ?? string.Empty
            );
        }

        /// <summary>
        /// Maps a CreatePersonCommand to a Person entity
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Person ToEntity(CreatePersonCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone if BirthDate is provided
            DateTime? birthDateUtc = null;
            if (command.BirthDate.HasValue)
            {
                birthDateUtc = command.BirthDate.Value.Kind switch
                {
                    DateTimeKind.Utc => command.BirthDate.Value,
                    DateTimeKind.Local => command.BirthDate.Value.ToUniversalTime(),
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(command.BirthDate.Value, DateTimeKind.Utc),
                    _ => DateTime.SpecifyKind(command.BirthDate.Value, DateTimeKind.Utc)
                };
            }

            Person person = new Person(
                command.FirstName,
                command.LastName,
                birthDateUtc,
                Domain.Enums.Common.PersonRole.User,
                ToAddress(command.Address),
                ToContactInfo(command.ContactInfo)
            );
            
            // Set the registration status
            person.UpdateIsRegistered(command.IsRegistered);
            
            return person;
        }
    }
}
