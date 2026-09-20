export function getRestApiUrl(): string {
  if (typeof window === 'undefined' && process.env.REST_API_URL) {
    return process.env.REST_API_URL;
  }

  return process.env.NEXT_PUBLIC_REST_API_URL ?? 'http://localhost:4001/api/v1';
}
