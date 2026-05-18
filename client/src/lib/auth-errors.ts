export class GrpcErrorMessage {
  public constructor(
    private readonly error: unknown,
    private readonly fallbackMessage: string,
  ) {}

  public get message(): string {
    if (
      this.error &&
      typeof this.error === 'object' &&
      'details' in this.error
    ) {
      const { details } = this.error as { details?: unknown };
      if (typeof details === 'string' && details.length > 0) {
        return details;
      }
    }

    return this.fallbackMessage;
  }
}
