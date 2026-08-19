using System.Reflection;
using Metaspesa.Application.Identity;
using Metaspesa.Domain.Identity;
using NetArchTest.Rules;

namespace Metaspesa.ArchitectureTests;

public class LayerDependencyTests {
  private static readonly Assembly DomainAssembly = typeof(User).Assembly;
  private static readonly Assembly ApplicationAssembly = typeof(LoginUser).Assembly;

  [Theory(DisplayName = "Domain has no dependency on outer server layers")]
  [InlineData("Metaspesa.Application")]
  [InlineData("Metaspesa.Database")]
  [InlineData("Metaspesa.Infrastructure")]
  [InlineData("Metaspesa.GrpcApi")]
  [InlineData("Metaspesa.MigrationService")]
  public void Domain_DoesNotDependOnOuterLayer(string forbiddenNamespace) =>
    AssertNoDependency(DomainAssembly, forbiddenNamespace);

  [Theory(DisplayName = "Application has no dependency on implementation layers")]
  [InlineData("Metaspesa.Database")]
  [InlineData("Metaspesa.Infrastructure")]
  [InlineData("Metaspesa.GrpcApi")]
  [InlineData("Metaspesa.MigrationService")]
  public void Application_DoesNotDependOnImplementationLayer(
    string forbiddenNamespace
  ) => AssertNoDependency(ApplicationAssembly, forbiddenNamespace);

  [Theory(DisplayName = "View layers expose no Domain response models")]
  [InlineData("GrpcApi", "Metaspesa.GrpcApi.Services")]
  [InlineData("MigrationService", "Metaspesa.MigrationService")]
  public void ViewLayer_DoesNotExposeDomainResponseModels(
    string assemblyName,
    string viewNamespace
  ) {
    var viewAssembly = Assembly.Load(assemblyName);
    Type[] domainResponseTypes = viewAssembly.GetTypes()
      .Where(type => type.Namespace == viewNamespace)
      .SelectMany(type => type.GetMethods(
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.DeclaredOnly))
      .SelectMany(method => ExpandType(method.ReturnType))
      .Where(type => type.Assembly == DomainAssembly)
      .Distinct()
      .ToArray();

    Assert.Empty(domainResponseTypes);
  }

  [Theory(DisplayName = "View layers have no dependency on other views")]
  [InlineData("GrpcApi", "Metaspesa.MigrationService")]
  [InlineData("MigrationService", "Metaspesa.GrpcApi")]
  public void ViewLayer_DoesNotDependOnOtherView(
    string assemblyName,
    string forbiddenNamespace
  ) => AssertNoDependency(Assembly.Load(assemblyName), forbiddenNamespace);

  private static void AssertNoDependency(
    Assembly assembly, string forbiddenNamespace
  ) {
    NetArchTest.Rules.TestResult result = Types.InAssembly(assembly)
      .ShouldNot()
      .HaveDependencyOn(forbiddenNamespace)
      .GetResult();

    Assert.True(
      result.IsSuccessful,
      $"Types depend on {forbiddenNamespace}: " +
      string.Join(", ", result.FailingTypeNames ?? []));
  }

  private static IEnumerable<Type> ExpandType(Type type) {
    yield return type;
    if (!type.IsGenericType) {
      yield break;
    }

    foreach (Type argument in type.GetGenericArguments()) {
      foreach (Type nestedType in ExpandType(argument)) {
        yield return nestedType;
      }
    }
  }
}