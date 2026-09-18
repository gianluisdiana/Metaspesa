using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Metaspesa.RestApi;

internal static class OpenApiExporter {
  public static async Task WriteOpenApiYamlAsync(
    this WebApplication app, string outputPath
  ) {
    app.Urls.Add("http://127.0.0.1:0");
    await app.StartAsync();
    try {
      IOpenApiDocumentProvider provider = app.Services
        .GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");
      OpenApiDocument document = await provider.GetOpenApiDocumentAsync(
        CancellationToken.None);
      if (document.Paths.Count == 0) {
        throw new InvalidOperationException("Generated OpenAPI document has no paths.");
      }

      string temporaryPath = $"{outputPath}.{Guid.NewGuid():N}.tmp";
      try {
        await using (FileStream stream = File.Create(temporaryPath)) {
          await document.SerializeAsYamlAsync(
            stream, OpenApiSpecVersion.OpenApi3_1, CancellationToken.None);
        }
        File.Move(temporaryPath, outputPath, overwrite: true);
      } finally {
        if (File.Exists(temporaryPath)) {
          File.Delete(temporaryPath);
        }
      }
    } finally {
      await app.StopAsync();
    }
  }
}