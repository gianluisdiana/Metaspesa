export default function DecorativeHeader() {
  return (
    <div className="h-32 bg-gradient-to-br from-primary-fixed-dim to-secondary-fixed-dim relative">
      <div className="absolute inset-0 bg-white/20 backdrop-blur-sm" />
      <div className="absolute inset-0 flex items-center justify-center">
        <span
          className="material-symbols-outlined text-on-primary-container text-5xl"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          storefront
        </span>
      </div>
    </div>
  );
}
