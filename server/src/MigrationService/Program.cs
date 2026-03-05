using Metaspesa.Database;
using Metaspesa.MigrationService;
using Metaspesa.ServiceDefaults;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.AddServiceDefaults();
builder.Services.AddDatabase();

IHost host = builder.Build();

await host.RunAsync();
