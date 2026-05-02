const VOLUME_HEIGHTS = ['40%', '60%', '45%', '80%', '65%', '90%'];

const PRICE_DOTS = [
  { cx: '15%', cy: '65%' },
  { cx: '30%', cy: '50%' },
  { cx: '45%', cy: '55%' },
  { cx: '60%', cy: '35%' },
  { cx: '75%', cy: '40%' },
  { cx: '90%', cy: '20%' },
];

function ChartHeader() {
  return (
    <div className="flex justify-between items-center">
      <h3 className="font-headline-md text-headline-md text-on-surface">
        Price &amp; Volume Trend
      </h3>
      <span className="bg-primary-container/20 text-on-primary-container px-3 py-1 rounded-full font-label-sm text-label-sm flex items-center gap-1">
        <span className="material-symbols-outlined text-[14px]">eco</span>
        {''}
        Organic Avocados
      </span>
    </div>
  );
}

function ChartArea() {
  return (
    <div className="relative h-64 w-full bg-surface-container-low rounded-lg p-4 flex items-end justify-between gap-2 overflow-hidden border border-outline-variant/20">
      <div className="absolute left-2 top-4 bottom-4 flex flex-col justify-between text-outline font-label-sm text-label-sm text-xs opacity-60">
        <span>$5.00</span>
        <span>$4.00</span>
        <span>$3.00</span>
        <span>$2.00</span>
      </div>
      <div className="ml-10 w-full h-full relative flex items-end justify-around">
        {VOLUME_HEIGHTS.map((h, i) => (
          <div
            key={i}
            className="w-[10%] bg-secondary-container/40 rounded-t-sm"
            style={{ height: h }}
          />
        ))}
        <svg
          className="absolute inset-0 w-full h-full"
          preserveAspectRatio="none"
        >
          <path
            className="text-primary-container"
            d="M 0,150 C 50,140 100,100 150,110 C 200,120 250,80 300,70 C 350,60 400,90 450,50 L 500,40"
            fill="none"
            stroke="currentColor"
            strokeWidth="3"
            vectorEffect="non-scaling-stroke"
          />
          {PRICE_DOTS.map((dot, i) => (
            <circle
              key={i}
              className="fill-primary"
              cx={dot.cx}
              cy={dot.cy}
              r="4"
            />
          ))}
        </svg>
      </div>
    </div>
  );
}

function ChartLegend() {
  return (
    <div className="flex justify-center gap-6 mt-2">
      <div className="flex items-center gap-2">
        <div className="w-3 h-3 rounded-full bg-primary-container" />
        <span className="font-label-sm text-label-sm text-on-surface-variant">
          Avg Price ($)
        </span>
      </div>
      <div className="flex items-center gap-2">
        <div className="w-3 h-3 rounded-sm bg-secondary-container/60" />
        <span className="font-label-sm text-label-sm text-on-surface-variant">
          Volume (k)
        </span>
      </div>
    </div>
  );
}

export default function ChartCard() {
  return (
    <div className="lg:col-span-2 bg-surface-container-lowest border border-outline-variant/30 rounded-xl p-6 shadow-[0_8px_32px_rgba(168,85,247,0.06)] flex flex-col gap-stack-md">
      <ChartHeader />
      <ChartArea />
      <ChartLegend />
    </div>
  );
}
