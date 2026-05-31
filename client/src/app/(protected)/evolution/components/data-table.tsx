export type MarketInfo = {
  colorClass: string;
  iconColorClass: string;
  name: string;
};

export type TrendInfo = { colorClass: string; icon: string; value: string };

export type TableRowData = {
  date: string;
  format: string;
  id: string;
  market: MarketInfo;
  price: string;
  pricePerUnit: string;
  trend: TrendInfo;
};

const TABLE_ROWS: TableRowData[] = [
  {
    date: 'Nov 12, 2023',
    format: '1L Bottle',
    id: '1',
    market: {
      colorClass: 'bg-tertiary-container/30',
      iconColorClass: 'text-tertiary',
      name: 'Supermarket A',
    },
    price: '$7.85',
    pricePerUnit: '$7.85/L',
    trend: {
      colorClass: 'text-tertiary',
      icon: 'trending_down',
      value: '2.4%',
    },
  },
  {
    date: 'Nov 10, 2023',
    format: '750ml Bottle',
    id: '2',
    market: {
      colorClass: 'bg-primary-container/30',
      iconColorClass: 'text-primary',
      name: 'Hypermarket B',
    },
    price: '$6.15',
    pricePerUnit: '$8.20/L',
    trend: {
      colorClass: 'text-on-surface-variant',
      icon: 'horizontal_rule',
      value: '0.0%',
    },
  },
  {
    date: 'Oct 28, 2023',
    format: '1L Bottle',
    id: '3',
    market: {
      colorClass: 'bg-secondary-container/30',
      iconColorClass: 'text-secondary',
      name: 'Supermarket A',
    },
    price: '$8.05',
    pricePerUnit: '$8.05/L',
    trend: { colorClass: 'text-error', icon: 'trending_up', value: '1.2%' },
  },
];

function TableHeader() {
  return (
    <div className="p-6 border-b border-outline-variant/20 flex justify-between items-center">
      <h3 className="font-headline-md text-headline-md text-on-surface">
        Price History
      </h3>
      <button className="flex items-center gap-2 px-4 py-2 bg-surface-container rounded-lg font-label-md text-label-md text-on-surface hover:bg-surface-container-high transition-colors">
        <span className="material-symbols-outlined text-[18px]">download</span>
        {''}
        Export CSV
      </button>
    </div>
  );
}

function MarketCell({ market }: Readonly<{ market: MarketInfo }>) {
  return (
    <td className="p-4">
      <div className="flex items-center gap-2">
        <div
          className={`w-6 h-6 rounded flex items-center justify-center ${market.colorClass}`}
        >
          <span
            className={`material-symbols-outlined text-[12px] ${market.iconColorClass}`}
          >
            store
          </span>
        </div>
        <span className="font-medium text-on-surface">{market.name}</span>
      </div>
    </td>
  );
}

function FormatCell({ format }: Readonly<{ format: string }>) {
  return (
    <td className="p-4 text-on-surface-variant">
      <div className="inline-flex rounded-md bg-surface-container px-2 py-1 text-sm text-on-surface">
        {format}
      </div>
    </td>
  );
}

function TrendCell({ trend }: Readonly<{ trend: TrendInfo }>) {
  return (
    <td
      className={`p-4 flex items-center gap-1 font-label-sm text-label-sm ${trend.colorClass}`}
    >
      <span className="material-symbols-outlined text-[16px]">
        {trend.icon}
      </span>
      {trend.value}
    </td>
  );
}

function TableRow({ row }: Readonly<{ row: TableRowData }>) {
  return (
    <tr className="border-b border-outline-variant/10 hover:bg-surface-container-low transition-colors last:border-b-0">
      <td className="p-4">{row.date}</td>
      <MarketCell market={row.market} />
      <FormatCell format={row.format} />
      <td className="p-4">{row.price}</td>
      <td className="p-4 font-medium text-primary">{row.pricePerUnit}</td>
      <TrendCell trend={row.trend} />
    </tr>
  );
}

export default function DataTable() {
  return (
    <div className="bg-surface-container-lowest border border-outline-variant/30 rounded-xl shadow-[0_4px_24px_rgba(168,85,247,0.04)] overflow-hidden">
      <TableHeader />
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-surface-container-lowest border-b border-outline-variant/20">
              {[
                'Date',
                'Market / Brand',
                'Format',
                'Price',
                'Price / Unit',
                'Trend',
              ].map(h => (
                <th
                  key={h}
                  className={`p-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider ${''}`}
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="font-body-md text-body-md text-on-surface">
            {TABLE_ROWS.map(row => (
              <TableRow key={row.id} row={row} />
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
