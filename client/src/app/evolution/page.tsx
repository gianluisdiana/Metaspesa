import BottomNav from '../components/bottom-nav';
import SideNav from '../components/side-nav';
import TopNav from '../components/top-nav';
import ChartCard from './components/chart-card';
import DataTable from './components/data-table';
import FiltersCard from './components/filters-card';
import MetricCards from './components/metric-cards';
import EvolutionPageHeader from './components/page-header';

export default function EvolutionPage() {
  return (
    <div className="bg-surface text-on-surface antialiased font-body-md text-body-md selection:bg-primary-container selection:text-on-primary-container">
      <TopNav />
      <SideNav activeHref="/evolution" />
      <main className="pt-[64px] pb-[80px] md:pb-8 md:ml-72 min-h-screen">
        <EvolutionPageHeader />
        <div className="p-container-margin">
          <FiltersCard />
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter mb-stack-lg">
            <ChartCard />
            <MetricCards />
          </div>
          <DataTable />
        </div>
      </main>
      <BottomNav activeHref="/evolution" />
    </div>
  );
}
