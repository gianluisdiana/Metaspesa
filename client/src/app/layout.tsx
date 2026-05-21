import type { Metadata } from 'next';
import { Geist, Geist_Mono } from 'next/font/google';

import {
  absoluteUrl,
  defaultDescription,
  seoKeywords,
  siteName,
  siteUrl,
} from '@/lib/seo';

import './globals.css';

const geistSans = Geist({
  subsets: ['latin'],
  variable: '--font-geist-sans',
});

const geistMono = Geist_Mono({
  subsets: ['latin'],
  variable: '--font-geist-mono',
});

const structuredData = [
  {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: siteName,
    url: siteUrl,
  },
  {
    '@context': 'https://schema.org',
    '@type': 'WebApplication',
    applicationCategory: 'ShoppingApplication',
    description: defaultDescription,
    inLanguage: 'es',
    name: siteName,
    operatingSystem: 'Web',
    url: siteUrl,
  },
];

export const metadata: Metadata = {
  alternates: {
    canonical: absoluteUrl('/'),
  },
  description: defaultDescription,
  keywords: seoKeywords,
  metadataBase: new URL(siteUrl),
  openGraph: {
    description: defaultDescription,
    locale: 'es_ES',
    siteName,
    title: siteName,
    type: 'website',
    url: siteUrl,
  },
  robots: {
    follow: true,
    index: true,
  },
  title: {
    default: siteName,
    template: `${siteName} | %s`,
  },
  twitter: {
    card: 'summary_large_image',
    description: defaultDescription,
    title: siteName,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link
          rel="preconnect"
          href="https://fonts.gstatic.com"
          crossOrigin="anonymous"
        />
        {/* eslint-disable-next-line @next/next/no-page-custom-font */}
        <link
          href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700&family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200&display=swap"
          rel="stylesheet"
        />
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: JSON.stringify(structuredData),
          }}
        />
      </head>
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased`}
      >
        {children}
      </body>
    </html>
  );
}
