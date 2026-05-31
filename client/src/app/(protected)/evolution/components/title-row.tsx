import TimeFilter from './time-filter';

export default function TitleRow() {
  return (
    <div className="mb-stack-lg flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
      <div>
        <div className="mb-1 flex items-center gap-2 text-on-surface-variant">
          <span className="font-label-sm text-label-sm font-bold uppercase tracking-wider">
            Evolution Trends
          </span>
        </div>
        <h1 className="font-headline-lg text-headline-lg text-on-surface">
          Organic Extra Virgin Olive Oil
        </h1>
        <p className="mt-1 font-body-md text-body-md text-on-surface-variant">
          Track price changes and compare formats across markets.
        </p>
      </div>
      <TimeFilter />
    </div>
  );
}
