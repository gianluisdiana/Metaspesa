import ChartCard from './components/chart-card';
import DataTable from './components/data-table';
import FiltersCard from './components/filters-card';
import MetricCards from './components/metric-cards';
import EvolutionPageHeader from './components/page-header';

export default function EvolutionPage() {
  return (
    <>
      <EvolutionPageHeader />
      <div className="p-container-margin">
        <FiltersCard />
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter mb-stack-lg">
          <ChartCard />
          <MetricCards />
        </div>
        <DataTable />
      </div>
    </>
  );
}
