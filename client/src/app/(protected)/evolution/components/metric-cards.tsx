type TrendBadgeProps = { colorClass: string; icon: string; value: string };

function TrendBadge({ colorClass, icon, value }: Readonly<TrendBadgeProps>) {
  return (
    <span
      className={`bg-surface-container flex items-center gap-1 px-2 py-1 rounded font-label-sm text-label-sm font-bold ${colorClass}`}
    >
      <span className="material-symbols-outlined text-[14px]">{icon}</span>
      {value}
    </span>
  );
}

function PriceMetricCard() {
  return (
    <div className="bg-surface-container-lowest border border-outline-variant/30 rounded-xl p-6 shadow-[0_4px_16px_rgba(168,85,247,0.03)] flex flex-col justify-between flex-1">
      <div className="flex justify-between items-start mb-4">
        <div className="w-10 h-10 rounded-full bg-primary-fixed/50 flex items-center justify-center text-primary">
          <span className="material-symbols-outlined">sell</span>
        </div>
        <TrendBadge colorClass="text-error" icon="trending_up" value="+12.5%" />
      </div>
      <div>
        <p className="font-label-sm text-label-sm text-on-surface-variant mb-1">
          Current Market Price
        </p>
        <div className="flex items-baseline gap-1">
          <span className="font-headline-lg text-headline-lg text-on-surface">
            $3.85
          </span>
          <span className="font-body-md text-body-md text-on-surface-variant">
            / unit
          </span>
        </div>
      </div>
    </div>
  );
}

function VolumeMetricCard() {
  return (
    <div className="bg-surface-container-lowest border border-outline-variant/30 rounded-xl p-6 shadow-[0_4px_16px_rgba(168,85,247,0.03)] flex flex-col justify-between flex-1">
      <div className="flex justify-between items-start mb-4">
        <div className="w-10 h-10 rounded-full bg-secondary-fixed/50 flex items-center justify-center text-secondary">
          <span className="material-symbols-outlined">inventory_2</span>
        </div>
        <TrendBadge
          colorClass="text-tertiary"
          icon="trending_down"
          value="-4.2%"
        />
      </div>
      <div>
        <p className="font-label-sm text-label-sm text-on-surface-variant mb-1">
          Available Quantity
        </p>
        <div className="flex items-baseline gap-1 mb-3">
          <span className="font-headline-lg text-headline-lg text-on-surface">
            14.2k
          </span>
          <span className="font-body-md text-body-md text-on-surface-variant">
            units
          </span>
        </div>
        <div className="w-full bg-surface-container-high rounded-full h-2 overflow-hidden">
          <div className="bg-linear-to-r from-tertiary-container to-primary-container h-full w-[65%] rounded-full" />
        </div>
      </div>
    </div>
  );
}

export default function MetricCards() {
  return (
    <div className="flex flex-col gap-gutter">
      <PriceMetricCard />
      <VolumeMetricCard />
    </div>
  );
}
