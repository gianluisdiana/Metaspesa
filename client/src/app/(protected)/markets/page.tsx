import RestShoppingApiService from '@/infrastructure/rest-shopping-api-service';
import { getAuthToken } from '@/lib/server/auth-cookie';

import FilterHeader from './components/filter-header';
import ProductGrid from './components/product-grid';

export default async function MarketsPage() {
  const token = await getAuthToken();
  const shoppingListSummaries = token
    ? await new RestShoppingApiService(token).getShoppingListSummaries()
    : [];

  return (
    <>
      <FilterHeader />
      <ProductGrid
        isAuthenticated={Boolean(token)}
        shoppingListSummaries={shoppingListSummaries}
      />
    </>
  );
}
