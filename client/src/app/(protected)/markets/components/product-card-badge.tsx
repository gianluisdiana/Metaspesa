export function ProductBadge({
  label,
  colorClass,
}: Readonly<{
  label: string;
  colorClass: string;
}>) {
  return (
    <div
      className={`absolute top-2 left-2 bg-white/90 backdrop-blur-sm px-2 py-0.5 rounded text-[10px] font-bold ${colorClass} shadow-sm tracking-wide uppercase`}
    >
      {label}
    </div>
  );
}
