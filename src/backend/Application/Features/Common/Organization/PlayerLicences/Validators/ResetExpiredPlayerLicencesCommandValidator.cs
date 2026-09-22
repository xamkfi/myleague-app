using Application.Features.Common.Organization.PlayerLicences.Commands;
using FluentValidation;

namespace Application.Features.Common.Organization.PlayerLicences.Validators;

public class ResetExpiredPlayerLicencesCommandValidator : AbstractValidator<ResetExpiredPlayerLicencesCommand>
{
    public ResetExpiredPlayerLicencesCommandValidator()
    {
    }
}
