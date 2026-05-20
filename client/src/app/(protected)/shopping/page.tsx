import GrpcApiService from '@/infrastructure/grpc-api-service';
import { PageSearchParams, stringParam } from '@/lib/search-params';
import { getAuthToken } from '@/lib/server/auth-cookie';

import ShoppingListContainer from './components/shopping-list-container';

export default async function ShoppingPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<PageSearchParams>;
}>) {
  const [params, token] = await Promise.all([searchParams, getAuthToken()]);
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
