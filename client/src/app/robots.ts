import type { MetadataRoute } from 'next';

import { absoluteUrl, siteUrl } from '@/lib/seo';

export default function robots(): MetadataRoute.Robots {
  return {
    host: siteUrl,
    rules: {
      allow: ['/', '/markets', '/evolution'],
      disallow: ['/shopping', '/auth', '/api'],
      userAgent: '*',
    },
    sitemap: absoluteUrl('/sitemap.xml'),
  };
}
