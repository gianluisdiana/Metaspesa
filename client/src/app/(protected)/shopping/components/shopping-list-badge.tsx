export function ItemBadge({
  colorClass,
  label,
}: Readonly<{ colorClass: string; label: string }>) {
  return (
    <span className={`px-2 py-0.5 rounded-md ${colorClass}`}>{label}</span>
  );
}
