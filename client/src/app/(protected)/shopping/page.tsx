import { cookies } from 'next/headers';

import GrpcApiService from '@/infrastructure/grpc-api-service';

import ShoppingListContainer from './components/shopping-list-container';

type SearchParams = { [key: string]: string | string[] | undefined };

function stringParam(params: SearchParams, key: string): string | undefined {
  const value = params[key];
  return typeof value === 'string' && value.length > 0 ? value : undefined;
}

export default async function ShoppingPage({
  searchParams,
}: Readonly<{
  searchParams: Promise<SearchParams>;
}>) {
  const [params, cookieStore] = await Promise.all([searchParams, cookies()]);
  const token = cookieStore.get('auth_token')?.value ?? '';
  const selectedListName = stringParam(params, 'name');
  const service = new GrpcApiService(token);
  const shoppingList = await service.getShoppingList(selectedListName);

  return (
    <ShoppingListContainer
      initialSelectedListName={selectedListName}
      initialShoppingList={shoppingList}
    />
  );
}
