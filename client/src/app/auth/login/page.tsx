import LeftVisualPanel from './components/left-visual-panel';
import RightLoginPanel from './components/right-login-panel';

export default function LoginPage() {
  return (
    <main className="bg-surface text-on-surface antialiased h-screen overflow-hidden flex flex-col md:flex-row">
      <LeftVisualPanel />
      <RightLoginPanel />
    </main>
  );
}
