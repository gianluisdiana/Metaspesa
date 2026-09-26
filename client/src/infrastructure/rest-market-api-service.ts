import {
  zGetMarketsResponse,
  zProductPageResponse,
} from '@/infrastructure/openapi_generated/zod.gen';
import MarketApiService, { MarketFilter } from '@/lib/market-api-service';
import {
  MarketProductsResult,
  MarketSummaryMessage,
} from '@/lib/market-contracts';
import { getRestApiUrl } from '@/lib/rest-api-url';

type Fetcher = (input: string, init: RequestInit) => Promise<Response>;

export default class RestMarketApiService implements MarketApiService {
  private readonly baseUrl: string;

  public constructor(
    baseUrl = getRestApiUrl(),
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
    const response = await this.request('/products', 'QUERY', filter);
    return zProductPageResponse.parse(await response.json());
  }

  private get(path: string): Promise<Response> {
    return this.request(path, 'GET');
  }

  private async request(
    path: string,
    method: string,
    body?: unknown,
  ): Promise<Response> {
    const response = await this.fetcher(`${this.baseUrl}${path}`, {
      body: body === undefined ? undefined : JSON.stringify(body),
      credentials: 'include',
      headers: {
        Accept: 'application/json',
        ...(body === undefined ? {} : { 'Content-Type': 'application/json' }),
      },
      method,
    });
    if (!response.ok) {
      throw new Error(`Market request failed (${response.status}).`);
    }
    return response;
  }
}
