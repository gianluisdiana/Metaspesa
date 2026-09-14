using Metaspesa.Application;
using Metaspesa.Database;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi;
using Metaspesa.RestApi.Identity;
using Metaspesa.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults("metaspesa-rest-api");

builder.Services
  .AddApplication()
  .AddPersistence()
  .AddInfrastructure()
  .AddRestApi(builder.Configuration);

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.MapIdentityEndpoints();
app.MapDefaultEndpoints();

await app.RunAsync();