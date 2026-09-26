using Metaspesa.Domain.SharedKernel;

namespace Metaspesa.Domain.UnitTests.SharedKernel;

public static class UidTests {
  [Fact]
  public static void Create_ReturnsVersion7Id() {
    Guid result = Uid.Create();

    Assert.Equal(7, result.Version);
  }
}