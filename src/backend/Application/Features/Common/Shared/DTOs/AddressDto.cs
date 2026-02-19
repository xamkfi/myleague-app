using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Common;

    /// <summary>
    /// Data Transfer Object for Address valueobject
    /// </summary>
    public record AddressDto(
        string? Street1,
        string? Street2,
        string? City,
        string? PostalCode,
        string Country);

