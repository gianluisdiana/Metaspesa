import type { MetadataRoute } from 'next';

import { absoluteUrl } from '@/lib/seo';

const PUBLIC_ROUTES = ['/markets', '/evolution'] as const;
const PUBLIC_PAGE_PRIORITY = 0.8;

export default function sitemap(): MetadataRoute.Sitemap {
  return PUBLIC_ROUTES.map(route => ({
    changeFrequency: 'daily',
    lastModified: new Date(),
    priority: PUBLIC_PAGE_PRIORITY,
    url: absoluteUrl(route),
  }));
}
