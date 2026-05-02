import ListTabs, { ListPageHeader } from './components/list-header';
import ItemsContainer, { ProgressTracker } from './components/list-items';
import SummaryFooter from './components/summary-footer';

export default function ShoppingPage() {
  return (
    <>
      <div className="top-16 z-30 bg-surface/90 backdrop-blur-md border-b border-surface-variant px-container-margin py-stack-md flex flex-col gap-stack-sm shadow-sm shadow-secondary/5">
        <ListPageHeader />
        <ListTabs />
      </div>
      <div className="p-container-margin">
        <ProgressTracker />
        <ItemsContainer />
      </div>
      <SummaryFooter />
    </>
  );
}
