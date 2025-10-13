using Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.CQRS
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
        Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);
    }
}
