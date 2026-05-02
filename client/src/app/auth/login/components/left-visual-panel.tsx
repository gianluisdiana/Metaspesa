const BG_IMAGE_URL =
  'https://lh3.googleusercontent.com/aida-public/AB6AXuCoGZHKySFqGO-UBUsKgXqmIXe7-0j6SWBRGzoP-NegmjOIK7ezeVLcRBxtrqzdtahOCAO7KroOiowibmVdHCXuYQuJkneWNP17segBmL6shELpJ2GsfGNGnfT6rlmWdZVTEsAhOCRazoKeWF1CeZrElCZP5T0rBDxRMCOKBKVUQsqeNfN-y2MSe6StCelQ0O68sc3AwU4sBoSFr8chVrhx1POY-53PaTx8RtpM8jcIMBxa4_anDAtndRUBgOg5I04dn8DCuv9xuTrb';

export function BackgroundImage() {
  return (
    <div
      className="absolute inset-0 z-0 bg-cover bg-center"
      style={{ backgroundImage: `url('${BG_IMAGE_URL}')` }}
    >
      <div className="absolute inset-0 bg-linear-to-t from-surface via-surface/60 to-transparent" />
    </div>
  );
}

export function BrandHeader() {
  return (
    <div className="relative z-10 p-stack-lg">
      <h1 className="font-headline-lg text-headline-lg text-primary tracking-tight">
        Metaspesa
      </h1>
      <p className="font-body-lg text-body-lg text-on-surface-variant mt-stack-sm max-w-md">
        Your serene grocery assistant. Transform the chore of shopping into a
        calm, organized experience.
      </p>
    </div>
  );
}

export function FreshnessBadge() {
  return (
    <div className="relative z-10 p-stack-lg">
      <div className="inline-flex items-center gap-2 bg-surface-container-low/80 backdrop-blur-md rounded-full px-4 py-2 border border-surface-variant shadow-sm shadow-secondary/5">
        <span
          className="material-symbols-outlined text-primary-fixed-dim"
          style={{ fontVariationSettings: "'FILL' 1" }}
        >
          eco
        </span>
        <span className="font-label-sm text-label-sm text-on-surface">
          Freshly organized
        </span>
      </div>
    </div>
  );
}

export default function LeftVisualPanel() {
  return (
    <div className="hidden md:flex md:w-1/2 lg:w-3/5 bg-surface-container relative overflow-hidden flex-col justify-between p-stack-lg border-r border-surface-variant">
      <BackgroundImage />
      <BrandHeader />
      <FreshnessBadge />
    </div>
  );
}
