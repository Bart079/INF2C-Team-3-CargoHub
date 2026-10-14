var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IClientService, ClientService>();

var app = builder.Build();

app.MapControllers();

app.Run();