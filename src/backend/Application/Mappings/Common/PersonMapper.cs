using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands.Persons;
using Application.DTOs.Common;
using Domain.Entities.Common;
using Domain.ValueObjects.Common;

namespace Application.Mappings.Common
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

            // Ensure DateTime is in UTC
            DateTime birthDateUtc = command.BirthDate.Kind switch
            {
                DateTimeKind.Utc => command.BirthDate,
                DateTimeKind.Local => command.BirthDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(command.BirthDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(command.BirthDate, DateTimeKind.Utc)
            };

            person.UpdateBasicInfo(command.FirstName, command.LastName);
            person.UpdateAddress(ToAddress(command.Address));
            person.UpdateContactInfo(ToContactInfo(command.ContactInfo));
            person.UpdateBirthDate(birthDateUtc);
            person.UpdateIsRegistered(command.IsRegistered);
        }


        /// <summary>
        /// Maps an AddressDto to an Address value object
        /// </summary>
        public static Address? ToAddress(AddressDto? dto)
        {
            if (dto == null) return null;
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
        /// Maps a ContactInfoDto to a ContactInfo value object
        /// </summary>
        public static ContactInfo? ToContactInfo(ContactInfoDto? dto)
        {
            if (dto == null) return null;
            return new ContactInfo(
                dto.Email ?? string.Empty,
                dto.Phone,
                dto.AlternativePhone
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

            // Ensure DateTime is in UTC to support PostgreSQL timestamp with time zone
            DateTime birthDateUtc = command.BirthDate.Kind switch
            {
                DateTimeKind.Utc => command.BirthDate,
                DateTimeKind.Local => command.BirthDate.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(command.BirthDate, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(command.BirthDate, DateTimeKind.Utc)
            };

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
