import 'server-only';

import { cookies } from 'next/headers';

export async function getAuthToken(): Promise<string> {
  const cookieStore = await cookies();
  return cookieStore.get('metaspesa_session')?.value ?? '';
}
