
using Application.Common;
using MediatR;

namespace Application.Features.Common.Images.Commands
{
    /// <summary>
    /// Command for uploading an image.
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="FileName"></param>
    /// <param name="ContentType"></param>
    public record UploadImageCommand(Stream imageStream, string FileName, string ContentType) : IRequest<Result<Uri>>; // Returns image URL as string
}
