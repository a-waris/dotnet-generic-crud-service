using GenericCRUDServiceExample;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(typeof(IGenericCRUDService<,>), typeof(GenericCRUDService<,>));
builder.Services.AddScoped<IContext, ApplicationDbContext>();
var app = builder.Build();

//
app.MapGet("/Hero", async (IContext db) => await db.Heroes.ToListAsync());
app.MapGet("/Hero{id}", async (IContext db, int id) => await db.Heroes.FindAsync(id));
app.MapPost("/Hero", () => async (IContext db, Hero hero) =>
{
    await db.Heroes.AddAsync(hero);
    await db.SaveChangesAsync();
    return Results.Created($"/Hero/{hero.Id}", hero);
});
app.MapPut("/Hero", () => async (IContext db, Hero hero) =>
{
    db.Heroes.Update(hero);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/{id}", () => async (IContext db, int id) =>
{
    var hero = await db.Heroes.FindAsync(id);
    if (hero == null)
    {
        return Results.NotFound();
    }

    db.Heroes.Remove(hero);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();