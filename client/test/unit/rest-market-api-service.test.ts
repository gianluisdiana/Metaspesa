import { describe, expect, it, vi } from 'vitest';
import { ZodError } from 'zod';

import RestMarketApiService from '@/infrastructure/rest-market-api-service';

describe('RestMarketApiService', () => {
  it('reads market items from REST API', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(
        Response.json({ items: [{ id: 1, name: 'Mercadona' }] }),
      );
    const service = new RestMarketApiService(
      'https://api.example/api/v1',
      fetcher,
    );

    const markets = await service.getMarkets();

    expect(markets).toEqual([{ id: 1, name: 'Mercadona' }]);
  });

  it('sends repeatable market IDs and all product filters', async () => {
    const fetcher = vi.fn().mockResolvedValue(
      Response.json({
        items: [],
        page: 2,
        pageSize: 24,
        totalItems: 0,
        totalPages: 0,
      }),
    );
    const service = new RestMarketApiService(
      'https://api.example/api/v1',
      fetcher,
    );

    await service.getMarketProducts({
      brand: 'Hacendado',
      marketId: [1, 2],
      page: 2,
      pageSize: 24,
      query: 'milk',
      sort: 'priceAsc',
    });

    expect(fetcher).toHaveBeenCalledWith(
      'https://api.example/api/v1/products?query=milk&marketId=1&marketId=2&brand=Hacendado&page=2&pageSize=24&sort=priceAsc',
      expect.objectContaining({ credentials: 'include', method: 'GET' }),
    );
  });

  it('rejects failed market responses', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(new Response(null, { status: 503 }));
    const service = new RestMarketApiService(
      'https://api.example/api/v1',
      fetcher,
    );

    await expect(service.getMarketProducts({})).rejects.toThrow(
      'Market request failed (503).',
    );
  });

  it('rejects a successful response that breaks the OpenAPI schema', async () => {
    const fetcher = vi
      .fn()
      .mockResolvedValue(Response.json({ items: [{ name: 'Mercadona' }] }));
    const service = new RestMarketApiService(
      'https://api.example/api/v1',
      fetcher,
    );

    await expect(service.getMarkets()).rejects.toBeInstanceOf(ZodError);
  });
});
