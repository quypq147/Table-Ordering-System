﻿using Infrastructure.DependencyInjection;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Application;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();  

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/health/db", async (TableOrderingDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("OK") : Results.Problem("Không thể kết nối tới Database"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TableOrderingDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

