import BottomNav from '../components/bottom-nav';
import SideNav from '../components/side-nav';
import TopNav from '../components/top-nav';
import ListTabs, { ListPageHeader } from './components/list-header';
import ItemsContainer, { ProgressTracker } from './components/list-items';
import SummaryFooter from './components/summary-footer';

export default function ShoppingPage() {
  return (
    <div className="bg-surface text-on-surface antialiased font-body-md text-body-md selection:bg-primary-container selection:text-on-primary-container">
      <TopNav />
      <SideNav activeHref="/shopping" />
      <main className="pt-[64px] pb-[80px] md:pb-8 md:ml-72 min-h-screen">
        <div className="top-[64px] z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
          <ListPageHeader />
          <ListTabs />
        </div>
        <div className="p-container-margin">
          <ProgressTracker />
          <ItemsContainer />
        </div>
        <SummaryFooter />
      </main>
      <BottomNav activeHref="/shopping" />
    </div>
  );
}
