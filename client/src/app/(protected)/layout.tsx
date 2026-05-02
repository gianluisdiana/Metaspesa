import BottomNav from './components/bottom-nav';
import SideNav from './components/side-nav';
import TopNav from './components/top-nav';

export default function ProtectedLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="bg-surface text-on-surface antialiased font-body-md text-body-md selection:bg-primary-container selection:text-on-primary-container">
      <TopNav />
      <SideNav />
      <main className="pt-16 pb-20 md:pb-8 md:ml-72 min-h-screen">
        {children}
      </main>
      <BottomNav />
    </div>
  );
}
