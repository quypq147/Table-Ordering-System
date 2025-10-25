﻿using Application.Abstractions;
using Application.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tables.Commands
{
    public sealed class MarkTableAvailableHandler : ICommandHandler<MarkTableAvailableCommand, TableDto>
    {
        private readonly IApplicationDbContext _db;
        public MarkTableAvailableHandler(IApplicationDbContext db) => _db = db;

        public async Task<TableDto> Handle(MarkTableAvailableCommand c, CancellationToken ct)
        {
            var t = await _db.Tables.FirstOrDefaultAsync(x => x.Id == c.Id, ct)
                    ?? throw new KeyNotFoundException("Table not found");
            t.MarkAvailable(); // :contentReference[oaicite:19]{index=19}
            await _db.SaveChangesAsync(ct);
            return new TableDto(t.Id, t.Code, t.Seats, t.Status);
        }
    }
}
