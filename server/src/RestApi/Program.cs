using Metaspesa.Application;
using Metaspesa.Database;
using Metaspesa.Infrastructure;
using Metaspesa.RestApi;
using Metaspesa.RestApi.Identity;
using Metaspesa.RestApi.Markets;
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
app.UseRequestDecompression();
app.UseCors();
if (app.Environment.IsDevelopment()) {
  app.MapOpenApi("/openapi/{documentName}.yaml");
}
app.MapIdentityEndpoints();
app.MapSnapshotEndpoint();
app.MapDefaultEndpoints();

if (args is ["--generate-openapi", string outputPath]) {
  await app.WriteOpenApiYamlAsync(outputPath);
  return;
}

await app.RunAsync();
