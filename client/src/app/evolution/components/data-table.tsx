export type MarketInfo = {
  colorClass: string;
  iconColorClass: string;
  name: string;
};

export type TrendInfo = { colorClass: string; icon: string; value: string };

export type TableRowData = {
  avgPrice: string;
  date: string;
  id: string;
  market: MarketInfo;
  trend: TrendInfo;
  volume: string;
};

const TABLE_ROWS: TableRowData[] = [
  {
    avgPrice: '$3.85',
    date: 'Oct 24, 2023',
    id: '1',
    market: {
      colorClass: 'bg-tertiary-container/30',
      iconColorClass: 'text-tertiary',
      name: 'Whole Foods',
    },
    trend: { colorClass: 'text-error', icon: 'arrow_upward', value: '+2.1%' },
    volume: '14.2k',
  },
  {
    avgPrice: '$3.77',
    date: 'Oct 17, 2023',
    id: '2',
    market: {
      colorClass: 'bg-primary-container/30',
      iconColorClass: 'text-primary',
      name: "Trader Joe's",
    },
    trend: {
      colorClass: 'text-tertiary',
      icon: 'arrow_downward',
      value: '-1.5%',
    },
    volume: '15.8k',
  },
  {
    avgPrice: '$3.82',
    date: 'Oct 10, 2023',
    id: '3',
    market: {
      colorClass: 'bg-secondary-container/30',
      iconColorClass: 'text-secondary',
      name: 'Safeway',
    },
    trend: { colorClass: 'text-error', icon: 'arrow_upward', value: '+0.8%' },
    volume: '13.5k',
  },
];

function TableHeader() {
  return (
    <div className="p-6 border-b border-outline-variant/20 flex justify-between items-center">
      <h3 className="font-headline-md text-headline-md text-on-surface">
        Historical Data Points
      </h3>
      <button className="flex items-center gap-2 px-4 py-2 bg-surface-container rounded-lg font-label-md text-label-md text-on-surface hover:bg-surface-container-high transition-colors">
        <span className="material-symbols-outlined text-[18px]">download</span>
        Export CSV
      </button>
    </div>
  );
}

function MarketCell({ market }: Readonly<{ market: MarketInfo }>) {
  return (
    <td className="p-4 flex items-center gap-2">
      <div
        className={`w-6 h-6 rounded flex items-center justify-center ${market.colorClass}`}
      >
        <span
          className={`material-symbols-outlined text-[12px] ${market.iconColorClass}`}
        >
          store
        </span>
      </div>
      {market.name}
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
      <td className="p-4 font-semibold">{row.avgPrice}</td>
      <td className="p-4">{row.volume}</td>
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
              {['Date', 'Market', 'Avg Price', 'Volume', 'Trend'].map(h => (
                <th
                  key={h}
                  className="p-4 font-label-sm text-label-sm text-on-surface-variant uppercase tracking-wider"
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
