import type { Metadata } from 'next';

import { pageMetadata } from '@/lib/seo';

import LeftVisualPanel from './components/left-visual-panel';
import RightLoginPanel from './components/right-login-panel';

export const metadata: Metadata = pageMetadata({
  canonicalPath: '/auth/login',
  noIndex: true,
  title: 'Iniciar sesion',
});

export default function LoginPage() {
  return (
    <main className="bg-surface text-on-surface antialiased h-screen overflow-hidden flex flex-col md:flex-row">
      <LeftVisualPanel />
      <RightLoginPanel />
    </main>
  );
}
