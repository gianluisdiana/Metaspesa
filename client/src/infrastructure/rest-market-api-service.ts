import {
  zGetMarketsResponse,
  zGetProductsResponse,
} from '@/infrastructure/openapi_generated/zod.gen';
import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-contracts';

type Fetcher = (input: string, init: RequestInit) => Promise<Response>;

export default class RestMarketApiService implements MarketApiService {
  private readonly baseUrl: string;

  public constructor(
    baseUrl = process.env.NEXT_PUBLIC_REST_API_URL ??
      'http://localhost:4001/api/v1',
    private readonly fetcher: Fetcher = globalThis.fetch.bind(globalThis),
  ) {
    this.baseUrl = baseUrl.replace(/\/$/, '');
  }

  public async getMarkets(): Promise<MarketSummaryMessage[]> {
    const response = await this.get('/markets');
    const body = zGetMarketsResponse.parse(await response.json());
    return body.items;
  }

  public async getMarketProducts(
    filter: MarketFilter = {},
  ): Promise<MarketProductsResult> {
    const params = new URLSearchParams();
    if (filter.query) params.set('query', filter.query);
    filter.marketId?.forEach(id => params.append('marketId', String(id)));
    if (filter.brand) params.set('brand', filter.brand);
    if (filter.page !== undefined) params.set('page', String(filter.page));
    if (filter.pageSize !== undefined) {
      params.set('pageSize', String(filter.pageSize));
    }
    if (filter.sort) params.set('sort', filter.sort);
    const query = params.size ? `?${params}` : '';
    const response = await this.get(`/products${query}`);
    return zGetProductsResponse.parse(await response.json());
  }

  private async get(path: string): Promise<Response> {
    const response = await this.fetcher(`${this.baseUrl}${path}`, {
      credentials: 'include',
      headers: { Accept: 'application/json' },
      method: 'GET',
    });
    if (!response.ok) {
      throw new Error(`Market request failed (${response.status}).`);
    }
    return response;
  }
}
