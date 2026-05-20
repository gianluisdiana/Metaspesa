import { cookies } from 'next/headers';

import GrpcApiService from '@/infrastructure/grpc-api-service';
import { PageSearchParams, stringParam } from '@/lib/search-params';

import ShoppingListContainer from './components/shopping-list-container';

export default async function ShoppingPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<PageSearchParams>;
}>) {
  const [params, cookieStore] = await Promise.all([searchParams, cookies()]);
  const token = cookieStore.get('auth_token')?.value ?? '';
  const selectedListName = stringParam(params, 'name');
  const service = new GrpcApiService(token);
  const [shoppingList, shoppingListSummaries] = await Promise.all([
    service.getShoppingList(selectedListName),
    service.getShoppingListSummaries(),
  ]);

  return (
    <ShoppingListContainer
      initialSelectedListName={selectedListName}
      initialShoppingList={shoppingList}
      initialShoppingListSummaries={shoppingListSummaries}
    />
  );
}
