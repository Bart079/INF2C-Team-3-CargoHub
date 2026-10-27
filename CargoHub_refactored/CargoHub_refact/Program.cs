using Project.Migration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IClientService, ClientService>();
var app = builder.Build();
// MigrationService.Run(); om nog een keer te migreren.
app.MapControllers();

app.Run();