using Application.DTOs.Common;
using DataImporter.Models;
using WebAPI.Models.Common;

namespace DataImporter;

public static class PersonMapper
{
    public static CreatePersonRequest MapToCreatePersonRequest(XmlPersonData xmlPerson)
    {
        // Parse birthdate - handle "0000-00-00" as DateTime.MinValue
        string birthDateStr = ParseBirthDate(xmlPerson.Birthday);

        // Determine IsRegistered based on country code "AFG"
        bool isRegistered = xmlPerson.Country == "AFG";

        // Map phone/mobile - use non-empty as Phone, other as AlternativePhone
        string? phone = GetNonEmptyPhone(xmlPerson.Phone, xmlPerson.Mobile);
        string? alternativePhone = GetAlternativePhone(xmlPerson.Phone, xmlPerson.Mobile);

        // Create AddressDto if we have any address data
        AddressDto? address = null;
        if (!string.IsNullOrWhiteSpace(xmlPerson.Address) ||
            !string.IsNullOrWhiteSpace(xmlPerson.Location) ||
            !string.IsNullOrWhiteSpace(xmlPerson.ZipCode) ||
            !string.IsNullOrWhiteSpace(xmlPerson.AddressCountry))
        {
            address = new AddressDto(
                xmlPerson.Address ?? string.Empty,
                xmlPerson.State ?? string.Empty,
                xmlPerson.Location ?? string.Empty,
                xmlPerson.ZipCode ?? string.Empty,
                xmlPerson.AddressCountry ?? string.Empty
            );
        }

        // Create ContactInfoDto if we have any contact data
        ContactInfoDto? contactInfo = null;
        if (!string.IsNullOrWhiteSpace(xmlPerson.Email) ||
            !string.IsNullOrWhiteSpace(phone) ||
            !string.IsNullOrWhiteSpace(alternativePhone))
        {
            contactInfo = new ContactInfoDto(
                xmlPerson.Email ?? string.Empty,
                phone ?? string.Empty,
                alternativePhone ?? string.Empty
            );
        }

        return new CreatePersonRequest
        {
            FirstName = xmlPerson.FirstName,
            LastName = xmlPerson.LastName,
            BirthDate = birthDateStr,
            IsRegistered = isRegistered,
            Address = address,
            ContactInfo = contactInfo
        };
    }

    private static string ParseBirthDate(string birthday)
    {
        if (string.IsNullOrWhiteSpace(birthday) || birthday == "0000-00-00")
        {
            return DateTime.MinValue.ToString("yyyy-MM-dd");
        }

        // Try to parse the date
        if (DateTime.TryParse(birthday, out DateTime parsedDate))
        {
            return parsedDate.ToString("yyyy-MM-dd");
        }

        // If parsing fails, return MinValue
        return DateTime.MinValue.ToString("yyyy-MM-dd");
    }

    private static string? GetNonEmptyPhone(string? phone, string? mobile)
    {
        if (!string.IsNullOrWhiteSpace(phone))
            return phone;
        if (!string.IsNullOrWhiteSpace(mobile))
            return mobile;
        return null;
    }

    private static string? GetAlternativePhone(string? phone, string? mobile)
    {
        if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(mobile))
            return mobile;
        return null;
    }
}

