// Application/Dtos/TableDto.cs
using Domain.Enums;

namespace Application.Dtos;

public sealed record TableDto(Guid Id, string Code, int Seats, TableStatus Status);

