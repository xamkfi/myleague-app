using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Common.Shared.DTOs;

    /// <summary>
    /// Data Transfer Object for ContactInfo valueobject
    /// </summary>
    public record ContactInfoDto(
        string? Email,
        string? Phone,
        string? AlternativePhone);

