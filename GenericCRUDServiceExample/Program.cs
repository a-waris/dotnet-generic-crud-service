using GenericCRUDServiceExample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(typeof(IGenericCRUDService<,>), typeof(GenericCRUDService<,>));
builder.Services.AddScoped<IContext, ApplicationDbContext>();
var app = builder.Build();

app.MapGet("/Hero", async (IGenericCRUDService<Hero, object> service) => await
    service.GetAll());

app.MapGet("/Hero{id}",
    async (IGenericCRUDService<Hero, object> service, HeroId id) => await service.GetById(x => x.Id == id));
app.MapPost("/Hero", () => async (IGenericCRUDService<Hero, object> service, Hero hero) =>
{
    await service.Add(hero);
    return Results.Created($"/Hero/{hero.Id}", hero);
});
app.MapPut("/Hero", () => async (IGenericCRUDService<Hero, object> service, Hero hero) =>
{
    await service.Update(hero);
    return Results.Created($"/Hero/{hero.Id}", hero);
});
app.MapDelete("/Hero{id}", () => async (IGenericCRUDService<Hero, object> service, HeroId id) =>
{
    await service.Delete(id);
    return Results.NoContent();
});


app.Run();