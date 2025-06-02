using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Common;

    /// <summary>
    /// Data Transfer Object for ContactInfo valueobject
    /// </summary>
    public record ContactInfoDto(
        string Email,
        string Phone,
        string? AlternativePhone);

