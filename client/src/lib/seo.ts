import type { Metadata } from 'next';

export const siteUrl = (
  process.env.NEXT_PUBLIC_SITE_URL ?? 'http://localhost:3000'
).replace(/\/$/, '');

export const siteName = 'Metaspesa';

export const seoKeywords = [
  'comparador precios supermercado',
  'lista compra',
  'ahorro cesta compra',
  'precios Mercadona',
  'precios Alcampo',
  'evolucion precios supermercado',
];

export const defaultDescription =
  'Compara precios de supermercados, sigue la evolucion de la cesta de la compra y organiza listas para comprar mejor.';

export function absoluteUrl(path = '/'): string {
  return new URL(path, siteUrl).toString();
}

export function pageMetadata({
  canonicalPath,
  description = defaultDescription,
  noIndex = false,
  title,
}: Readonly<{
  canonicalPath: string;
  description?: string;
  noIndex?: boolean;
  title: string;
}>): Metadata {
  const url = absoluteUrl(canonicalPath);

  return {
    alternates: {
      canonical: url,
    },
    description,
    keywords: seoKeywords,
    openGraph: {
      description,
      siteName,
      title: `${siteName} | ${title}`,
      type: 'website',
      url,
    },
    robots: {
      follow: !noIndex,
      index: !noIndex,
    },
    title,
    twitter: {
      card: 'summary_large_image',
      description,
      title: `${siteName} | ${title}`,
    },
  };
}
