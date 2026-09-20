import type { Metadata } from 'next';

import RestShoppingApiService from '@/infrastructure/rest-shopping-api-service';
import { PageSearchParams, stringParam } from '@/lib/search-params';
import { pageMetadata } from '@/lib/seo';
import { getAuthToken } from '@/lib/server/auth-cookie';

import ShoppingListContainer from './components/shopping-list-container';
import { loadShoppingPage } from './shopping-page-loader';

export const metadata: Metadata = pageMetadata({
  canonicalPath: '/shopping',
  noIndex: true,
  title: 'Lista de compra',
});

export default async function ShoppingPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<PageSearchParams>;
}>) {
  const [params, token] = await Promise.all([searchParams, getAuthToken()]);
  const idParam = Number(stringParam(params, 'listId'));
  const selectedListId =
    Number.isSafeInteger(idParam) && idParam > 0 ? idParam : undefined;
  const service = new RestShoppingApiService(token);
  const pageData = await loadShoppingPage(service, selectedListId);

  return (
    <ShoppingListContainer
      initialSelectedListId={pageData.selectedListId}
      initialShoppingList={pageData.shoppingList}
      initialShoppingListSummaries={pageData.shoppingListSummaries}
    />
  );
}
