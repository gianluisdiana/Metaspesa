import type { Metadata } from 'next';

import GrpcApiService from '@/infrastructure/grpc-api-service';
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
  const selectedListName = stringParam(params, 'name');
  const service = new GrpcApiService(token);
  const pageData = await loadShoppingPage(service, selectedListName);

  return (
    <ShoppingListContainer
      initialSelectedListName={pageData.selectedListName}
      initialShoppingList={pageData.shoppingList}
      initialShoppingListSummaries={pageData.shoppingListSummaries}
    />
  );
}
