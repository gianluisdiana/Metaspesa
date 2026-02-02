import GrpcApiService from '@/infrastructure/grpc-api-service';
import ApiService from '@/lib/api-service';
import { ShoppingListMessage } from '@/lib/messages';

import { recordShoppingList } from './actions';
import ShoppingClient from './components/shopping-client';

export const dynamic = 'force-dynamic';

export default async function Home() {
  const apiService: ApiService = new GrpcApiService();
  const shoppingList: ShoppingListMessage =
    await apiService.getCurrentShoppingList();

  return (
    <ShoppingClient
      initialShoppingList={shoppingList}
      onRecord={recordShoppingList}
    />
  );
}
