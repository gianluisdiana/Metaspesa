using Metaspesa.Application.Abstractions.Core;

namespace Metaspesa.Infrastructure;

internal class SystemClock : IClock {
  public DateTime GetCurrentTime() => DateTime.UtcNow;
}
