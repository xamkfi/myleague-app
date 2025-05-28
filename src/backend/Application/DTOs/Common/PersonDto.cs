using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.ValueObjects.Common;

namespace Application.DTOs.Common;

    /// <summary>
    /// Data Transfer Object for Person Entity
    /// </summary>
    public record PersonDto(
        Guid Id,
        string FirstName,
        string LastName,
        DateTime BirthDate, 
        string FullName,
        Address? Address,
        ContactInfo? ContactInfo);

