type SummaryCardProps = {
  icon: string;
  market: string;
  price: string;
  trend: {
    colorClass: string;
    icon: string;
    value: string;
  };
  unit: string;
};

function SummaryCard({
  icon,
  market,
  price,
  trend,
  unit,
}: Readonly<SummaryCardProps>) {
  return (
    <div className="group relative overflow-hidden rounded-xl border border-outline-variant/30 bg-surface-container-lowest p-5 shadow-[0_4px_16px_rgba(168,85,247,0.03)]">
      <div className="absolute top-0 right-0 p-4 opacity-10 transform translate-x-2 -translate-y-2 group-hover:scale-110 transition-transform">
        <span className="material-symbols-outlined text-[80px]">{icon}</span>
      </div>
      <h4 className="font-label-bold text-on-surface-variant mb-4 tracking-wider flex items-center gap-2">
        {market}
      </h4>
      <div className="flex items-end justify-between gap-4">
        <div>
          <span className="block font-display-lg text-display-lg leading-none text-on-surface">
            {price}
          </span>
          <span className="mt-1 block font-body-sm text-body-sm text-on-surface-variant">
            {unit}
          </span>
        </div>
        <div
          className={`flex items-center gap-1 rounded bg-surface-container px-2 py-1 ${trend.colorClass}`}
        >
          <span className="material-symbols-outlined text-[14px]">
            {trend.icon}
          </span>
          <span className="font-label-sm text-label-sm font-bold">
            {trend.value}
          </span>
        </div>
      </div>
    </div>
  );
}

export default function SummaryCards() {
  return (
    <div className="flex flex-col gap-gutter">
      <SummaryCard
        icon="store"
        market="FreshMarket"
        price="$14.46"
        trend={{
          colorClass: 'text-tertiary',
          icon: 'trending_down',
          value: '2.4%',
        }}
        unit="per 1 Liter"
      />
      <SummaryCard
        icon="domain"
        market="EcoGrocer"
        price="$17.00"
        trend={{
          colorClass: 'text-error',
          icon: 'trending_up',
          value: '1.5%',
        }}
        unit="per 1 Liter"
      />
    </div>
  );
}
