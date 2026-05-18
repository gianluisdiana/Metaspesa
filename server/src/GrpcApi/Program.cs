using Metaspesa.Application;
using Metaspesa.Database;
using Metaspesa.GrpcApi;
using Metaspesa.Infrastructure;
using Metaspesa.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("metaspesa-grpc-server");

builder.Services
  .AddApplication()
  .AddPersistence()
  .AddInfrastructure()
  .AddGrpcApi();

WebApplication app = builder.Build();

app.AddCustomMiddleware();
app.MapGrpcServices();

if (app.Environment.IsDevelopment()) {
  app.MapGrpcReflectionService();
}

await app.RunAsync();