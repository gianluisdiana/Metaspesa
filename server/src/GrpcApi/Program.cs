using Metaspesa.Application;
using Metaspesa.Database;
using Metaspesa.GrpcApi.Services;
using Metaspesa.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
  .AddApplication()
  .AddDatabase()
  .AddGrpc();

WebApplication app = builder.Build();

app.MapGrpcService<ShoppingGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
