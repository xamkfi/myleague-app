using Application.Common;
using Application.DTOs.Common;
using Application.Features.Common.FooterContacts.Mappings;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using MediatR;

namespace Application.Features.Common.FooterContacts.Commands;

public record UpdateFooterContactCommand(
    Guid Id,
    string Title,
    string? Details,
    string? Email,
    string? Phone,
    string? Url,
    int SortOrder,
    FooterSection Section,
    string? LastModifiedBy
) : IRequest<Result<FooterContactDto>>;

public class UpdateFooterContactCommandHandler
    : IRequestHandler<UpdateFooterContactCommand, Result<FooterContactDto>>
{
    private readonly IFooterContactRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFooterContactCommandHandler(
        IFooterContactRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FooterContactDto>> Handle(
        UpdateFooterContactCommand request,
        CancellationToken cancellationToken)
    {
        FooterContact? entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result<FooterContactDto>.NotFound("FooterContact", request.Id);
        }

        try
        {
            entity.Update(
                request.Title,
                request.Details,
                request.Email,
                request.Phone,
                FooterContactMapper.ParseUrl(request.Url),
                request.SortOrder,
                request.Section,
                request.LastModifiedBy);

            await _repository.UpdateAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<FooterContactDto>.Success(FooterContactMapper.ToDto(entity));
        }
        catch (ArgumentException ex)
        {
            return Result<FooterContactDto>.Failure(ex.Message);
        }
    }
}
