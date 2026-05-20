import { MarketFilter } from '@/lib/market-api-service';
import { positiveNumberParam, stringParam } from '@/lib/search-params';

const DEFAULT_PAGE = 1;
const DEFAULT_PAGE_SIZE = 20;

export class MarketProductsRequest {
  public constructor(private readonly params: URLSearchParams) {}

  public toFilter(): MarketFilter {
    return {
      brandNameSegment: this.stringParam('brand_name'),
      marketName: this.stringParam('market_name'),
      nameSegment: this.stringParam('name_segment'),
      page: this.positiveNumberParam('page', DEFAULT_PAGE),
      pageSize: this.positiveNumberParam('page_size', DEFAULT_PAGE_SIZE),
    };
  }

  private positiveNumberParam(key: string, fallback: number): number {
    return positiveNumberParam(this.params, key, fallback);
  }

  private stringParam(key: string): string | undefined {
    return stringParam(this.params, key);
  }
}
