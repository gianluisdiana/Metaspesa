import { cookies } from 'next/headers';

import GrpcApiService from '@/infrastructure/grpc-api-service';

import ShoppingListContainer from './components/shopping-list-container';

export default async function ShoppingPage() {
  const cookieStore = await cookies();
  const token = cookieStore.get('auth_token')?.value ?? '';
  const service = new GrpcApiService(token);
  const shoppingList = await service.getCurrentShoppingList();

  return <ShoppingListContainer initialShoppingList={shoppingList} />;
}
