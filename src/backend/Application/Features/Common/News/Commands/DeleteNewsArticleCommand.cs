// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using MediatR;

namespace Application.Commands.NewsArticles;

    /// <summary>
    /// Command for deleting a news article
    /// </summary>
    /// <param name="id"></param>
    public record DeleteNewsArticleCommand(Guid id) : IRequest<Result<bool>>;

