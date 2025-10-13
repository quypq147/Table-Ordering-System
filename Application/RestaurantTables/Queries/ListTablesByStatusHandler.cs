using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.RestaurantTables.Queries
{
    public sealed class ListTablesByStatusHandler
    : IQueryHandler<ListTablesByStatusQuery, IReadOnlyList<TableDto>>
    {
        private readonly IApplicationDbContext _db;
        public ListTablesByStatusHandler(IApplicationDbContext db) => _db = db;

        public async Task<IReadOnlyList<TableDto>> Handle(ListTablesByStatusQuery q, CancellationToken ct)
        {
            var rows = await _db.RestaurantTables
                .Where(t => t.Status.ToString() == q.Status)
                .OrderBy(t => t.Code)
                .ToListAsync(ct);

            return rows.Select(t => new TableDto(t.Id, t.Code, t.Seats, t.Status)).ToList();
        }
    }


}
