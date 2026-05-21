import type { Metadata } from 'next';

import { pageMetadata } from '@/lib/seo';

import RegisterCard from './components/register-card';

export const metadata: Metadata = pageMetadata({
  canonicalPath: '/auth/register',
  noIndex: true,
  title: 'Crear cuenta',
});

export default function RegisterPage() {
  return (
    <main className="bg-surface text-on-surface h-screen overflow-hidden flex items-center justify-center p-4">
      <RegisterCard />
    </main>
  );
}
