import ListTabs, { ListPageHeader } from './components/list-header';
import ItemsContainer, { ProgressTracker } from './components/list-items';
import ShoppingBottomNav from './components/shopping-bottom-nav';
import ShoppingHeader from './components/shopping-header';
import ShoppingSideNav from './components/shopping-side-nav';
import SummaryFooter from './components/summary-footer';

export default function ShoppingPage() {
  return (
    <div className="bg-surface text-on-surface font-body-md antialiased overflow-hidden flex h-screen selection:bg-primary-container selection:text-on-primary-container">
      <ShoppingSideNav />
      <main className="flex-1 flex flex-col h-full relative overflow-hidden">
        <ShoppingHeader />
        <div className="flex-1 overflow-y-auto pt-24 pb-32 md:pb-8 px-container-margin md:px-8">
          <div className="max-w-4xl mx-auto flex flex-col gap-stack-lg">
            <div className="flex flex-col gap-stack-md">
              <ListPageHeader />
              <ListTabs />
            </div>
            <ProgressTracker />
            <ItemsContainer />
          </div>
        </div>
        <SummaryFooter />
      </main>
      <ShoppingBottomNav />
    </div>
  );
}
