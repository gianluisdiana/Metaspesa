using Metaspesa.Application;
using Metaspesa.Database;
using Metaspesa.GrpcApi.Services;
using Metaspesa.Infrastructure;
using Metaspesa.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("metaspesa-grpc-server");

builder.Services
  .AddApplication()
  .AddDatabase()
  .AddInfrastructure()
  .AddGrpc();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<AuthGrpcService>();
app.MapGrpcService<ShoppingGrpcService>();
app.MapGrpcService<MarketGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
