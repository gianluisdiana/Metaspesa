export function getAuthErrorMessage(
  error: unknown,
  fallbackMessage: string,
): string {
  return error instanceof Error && error.message.length > 0
    ? error.message
    : fallbackMessage;
}
