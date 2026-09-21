using Application.Features.Common.PlayerLicences.Commands;
using FluentValidation;

namespace Application.Features.Common.PlayerLicences.Validators;

public class ResetExpiredPlayerLicencesCommandValidator : AbstractValidator<ResetExpiredPlayerLicencesCommand>
{
    public ResetExpiredPlayerLicencesCommandValidator()
    {
    }
}
