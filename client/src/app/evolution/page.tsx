import SideNav from '../components/side-nav';
import TopNav from '../components/top-nav';
import ChartCard from './components/chart-card';
import DataTable from './components/data-table';
import EvolutionBottomNav from './components/evolution-bottom-nav';
import FiltersCard from './components/filters-card';
import MetricCards from './components/metric-cards';
import EvolutionPageHeader from './components/page-header';

export default function EvolutionPage() {
  return (
    <div className="bg-background text-on-background min-h-screen font-body-md text-body-md">
      <SideNav activeHref="/evolution" />
      <TopNav />
      <main className="flex-1 w-full pt-20 md:pl-72 pb-24 md:pb-8 px-container-margin md:px-8 max-w-7xl mx-auto">
        <EvolutionPageHeader />
        <FiltersCard />
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter mb-stack-lg">
          <ChartCard />
          <MetricCards />
        </div>
        <DataTable />
      </main>
      <EvolutionBottomNav />
    </div>
  );
}
