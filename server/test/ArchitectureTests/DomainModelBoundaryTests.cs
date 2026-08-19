using System.Reflection;
using Metaspesa.Domain.Markets;
using Metaspesa.Domain.Shopping;

namespace Metaspesa.ArchitectureTests;

public class DomainModelBoundaryTests {
  [Fact(DisplayName = "Shopping references no Markets aggregates")]
  public void Shopping_DoesNotReferenceMarketsAggregates() {
    var forbiddenTypes = new HashSet<Type> {
      typeof(Market),
      typeof(Product),
      typeof(ProductFormat),
      typeof(PriceSnapshot),
    };

    Type[] violations = typeof(ShoppingList).Assembly.GetTypes()
      .Where(type => type.Namespace?.StartsWith(
        "Metaspesa.Domain.Shopping",
        StringComparison.Ordinal) == true)
      .Where(type => ReferencedTypes(type).Any(forbiddenTypes.Contains))
      .ToArray();

    Assert.Empty(violations);
  }

  [Fact(DisplayName = "Product aggregate owns no PriceSnapshot history")]
  public void Product_DoesNotReferencePriceSnapshotHistory() {
    Type[] aggregateTypes = [typeof(Product), typeof(ProductFormat)];

    Assert.DoesNotContain(
      aggregateTypes,
      type => ReferencedTypes(type).Contains(typeof(PriceSnapshot)));
  }

  private static IEnumerable<Type> ReferencedTypes(Type type) {
    const BindingFlags flags =
      BindingFlags.Instance |
      BindingFlags.Static |
      BindingFlags.Public |
      BindingFlags.NonPublic |
      BindingFlags.DeclaredOnly;

    IEnumerable<Type> directTypes = type.GetFields(flags)
      .Select(field => field.FieldType)
      .Concat(type.GetProperties(flags).Select(property => property.PropertyType))
      .Concat(type.GetConstructors(flags)
        .SelectMany(constructor => constructor.GetParameters())
        .Select(parameter => parameter.ParameterType))
      .Concat(type.GetMethods(flags).Select(method => method.ReturnType))
      .Concat(type.GetMethods(flags)
        .SelectMany(method => method.GetParameters())
        .Select(parameter => parameter.ParameterType));

    return directTypes.SelectMany(ExpandType).Distinct();
  }

  private static IEnumerable<Type> ExpandType(Type type) {
    Type normalizedType = type.HasElementType
      ? type.GetElementType()!
      : type;
    yield return normalizedType;

    if (!normalizedType.IsGenericType) {
      yield break;
    }

    foreach (Type argument in normalizedType.GetGenericArguments()) {
      foreach (Type nestedType in ExpandType(argument)) {
        yield return nestedType;
      }
    }
  }
}