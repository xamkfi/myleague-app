using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;

namespace Application.Features.Common.Images.Commands
{
    /// <summary>
    /// Command for deleting a image
    /// </summary>
    /// <param name="url"></param>
    public record DeleteImageCommand(Uri url) : IRequest<Result<bool>>; 
}
