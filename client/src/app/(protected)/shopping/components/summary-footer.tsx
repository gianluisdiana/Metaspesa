export function CheckedTotal({ total }: Readonly<{ total: string }>) {
  return (
    <div className="flex flex-col">
      <span className="font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1">
        <span className="material-symbols-outlined text-[16px] text-primary">
          check_circle
        </span>
        {''}
        Checked Items
      </span>
      <span className="font-body-md text-body-md text-on-surface mt-0.5">
        {total}
      </span>
    </div>
  );
}

export function EstimatedTotal({ total }: Readonly<{ total: string }>) {
  return (
    <div className="flex flex-col items-end flex-1">
      <span className="font-label-md text-label-md text-on-surface-variant">
        Estimated Total
      </span>
      <span className="font-headline-lg text-headline-lg text-primary tracking-tight leading-none mt-1">
        {total}
      </span>
    </div>
  );
}

export default function SummaryFooter({
  checkedTotal,
  estimatedTotal,
}: Readonly<{ checkedTotal: string; estimatedTotal: string }>) {
  return (
    <div className="fixed bottom-22 md:bottom-stack-lg left-0 right-0 md:left-72 z-30 pointer-events-none flex justify-center px-container-margin">
      <div className="pointer-events-auto bg-surface-container-lowest/80 backdrop-blur-xl border border-surface-container-highest shadow-[0_8px_32px_rgba(208,197,253,0.15)] w-full max-w-2xl rounded-2xl p-stack-md flex justify-between items-center">
        <CheckedTotal total={checkedTotal} />
        <div className="h-10 w-px bg-outline-variant/40 mx-4" />
        <EstimatedTotal total={estimatedTotal} />
      </div>
    </div>
  );
}
