﻿// Api/Controllers/CategoriesController.cs
using Application.Abstractions;
using Application.Categories.Commands;
using Application.Categories.Queries;
using Application.Common.CQRS;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;
    public CategoriesController(ISender sender) => _sender = sender;

    [HttpPost]
    public Task<CategoryDto> Create(CreateCategoryCommand cmd, CancellationToken ct)
        => _sender.Send(cmd, ct);

    [HttpPut("{id}/rename")]
    public Task<CategoryDto> Rename(string id, [FromBody] string newName, CancellationToken ct)
        => _sender.Send(new RenameCategoryCommand(id, newName), ct);

    [HttpPut("{id}/description")]
    public Task<CategoryDto> ChangeDescription(string id, [FromBody] string? desc, CancellationToken ct)
        => _sender.Send(new ChangeCategoryDescriptionCommand(id, desc), ct);

    [HttpPut("{id}/sort")]
    public Task<CategoryDto> ChangeSort(string id, [FromBody] int sort, CancellationToken ct)
        => _sender.Send(new ChangeCategorySortOrderCommand(id, sort), ct);

    [HttpPost("{id}/activate")]
    public Task<CategoryDto> Activate(string id, CancellationToken ct)
        => _sender.Send(new ActivateCategoryCommand(id), ct);

    [HttpPost("{id}/deactivate")]
    public Task<CategoryDto> Deactivate(string id, CancellationToken ct)
        => _sender.Send(new DeactivateCategoryCommand(id), ct);

    [HttpGet("{id}")]
    public Task<CategoryDto?> GetById(string id, CancellationToken ct)
        => _sender.Send(new GetCategoryByIdQuery(id), ct);

    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> List([FromQuery] string? search, [FromQuery] bool? onlyActive,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => _sender.Send(new ListCategoriesQuery(search, onlyActive, page, pageSize), ct);
}

