import BottomNav from '../components/bottom-nav';
import SideNav from '../components/side-nav';
import TopNav from '../components/top-nav';
import FilterHeader from './components/filter-header';
import ProductGrid from './components/product-grid';

export default function MarketsPage() {
  return (
    <div className="bg-surface text-on-surface antialiased font-body-md text-body-md selection:bg-primary-container selection:text-on-primary-container">
      <TopNav />
      <SideNav />
      <main className="pt-[64px] pb-[80px] md:pb-8 md:ml-72 min-h-screen">
        <FilterHeader />
        <ProductGrid />
      </main>
      <BottomNav />
    </div>
  );
}
