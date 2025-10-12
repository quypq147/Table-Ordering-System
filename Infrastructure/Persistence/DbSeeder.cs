using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(TableOrderingDbContext db)
    {
        if (!await db.MenuItems.AnyAsync())
        {
            db.MenuItems.AddRange(
                new MenuItem("M01", "Phở bò", new Money(45000, "VND")),
                new MenuItem("M02", "Bánh mì", new Money(25000, "VND")),
                new MenuItem("M03", "Cà phê sữa", new Money(20000, "VND"))
            );
        }

        if (!await db.Tables.AnyAsync())
        {
            db.Tables.AddRange(
                new RestaurantTable("T01", "T01", 4),
                new RestaurantTable("T02", "T02", 2),
                new RestaurantTable("T03", "T03", 6)
            );
        }

        await db.SaveChangesAsync();
    }
}


