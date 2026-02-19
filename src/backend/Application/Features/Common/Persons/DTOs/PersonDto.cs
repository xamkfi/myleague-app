using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.ValueObjects.Common;
using Domain.Enums.Common;

namespace Application.Features.Common.Persons.DTOs;

    /// <summary>
    /// Data Transfer Object for Person Entity
    /// </summary>
    public record PersonDto(
        Guid Id,
        string FirstName,
        string LastName,
        DateTime? BirthDate, 
        string FullName,
        PersonRole Role,
        bool IsRegistered,
        Address? Address,
        ContactInfo? ContactInfo);

