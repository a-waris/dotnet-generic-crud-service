using GenericCRUDServiceExample;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(typeof(IGenericCRUDService<,>), typeof(GenericCRUDService<,>));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();