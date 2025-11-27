using System.Xml;
using DataImporter.Models;

namespace DataImporter;

public static class JlgFileReader
{
    public static List<XmlPersonData> ReadJlgFile(string filePath)
    {
        List<XmlPersonData> persons = new List<XmlPersonData>();

        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        XmlNodeList? personRecords = doc.SelectNodes("//record[@object='Person']");
        
        if (personRecords == null || personRecords.Count == 0)
        {
            return persons;
        }

        foreach (XmlNode personNode in personRecords)
        {
            XmlPersonData person = ParsePersonNode(personNode);
            persons.Add(person);
        }

        return persons;
    }

    public static XmlPersonData ParsePersonNode(XmlNode personNode)
    {
        XmlPersonData person = new XmlPersonData();

        person.FirstName = GetCDataValue(personNode, "firstname") ?? string.Empty;
        person.LastName = GetCDataValue(personNode, "lastname") ?? string.Empty;
        person.Birthday = GetCDataValue(personNode, "birthday") ?? string.Empty;
        person.Country = GetCDataValue(personNode, "country") ?? string.Empty;
        person.Email = GetCDataValue(personNode, "email") ?? string.Empty;
        person.Phone = GetCDataValue(personNode, "phone") ?? string.Empty;
        person.Mobile = GetCDataValue(personNode, "mobile") ?? string.Empty;
        person.Address = GetCDataValue(personNode, "address") ?? string.Empty;
        person.ZipCode = GetCDataValue(personNode, "zipcode") ?? string.Empty;
        person.Location = GetCDataValue(personNode, "location") ?? string.Empty;
        person.State = GetCDataValue(personNode, "state") ?? string.Empty;
        person.AddressCountry = GetCDataValue(personNode, "address_country") ?? string.Empty;

        return person;
    }

    private static string? GetCDataValue(XmlNode parentNode, string elementName)
    {
        XmlNode? node = parentNode.SelectSingleNode(elementName);
        if (node != null && node.FirstChild is XmlCDataSection cdata)
        {
            return cdata.Value;
        }
        return null;
    }
}

