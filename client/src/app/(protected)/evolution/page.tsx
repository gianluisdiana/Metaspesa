import ChartCard from './components/chart-card';
import DataTable from './components/data-table';
import EvolutionPageHeader from './components/page-header';
import SummaryCards from './components/summary-cards';
import TitleRow from './components/title-row';

export default function EvolutionPage() {
  return (
    <>
      <EvolutionPageHeader />
      <div className="p-container-margin">
        <TitleRow />
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter mb-stack-lg">
          <ChartCard />
          <SummaryCards />
        </div>
        <DataTable />
      </div>
    </>
  );
}
