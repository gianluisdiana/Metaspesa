import { Product, ShoppingList } from '@/lib/domain';

export default function ShoppingListTable ({
  shoppingList,
  onToggle,
}: Readonly<{
  shoppingList: ShoppingList;
  onToggle: (name: string) => void;
}>) {
  return (
    <table>
      <thead>
        <ShoppingListHeader />
      </thead>
      <tbody>
        {shoppingList.products.map(product => (
          <ShoppingListProductRow
            key={product.name}
            product={product}
            onToggle={onToggle}
          />
        ))}
      </tbody>
      <tfoot>
        <ShoppingListTotalRow shoppingList={shoppingList} />
      </tfoot>
    </table>
  );
};

const ShoppingListProductRow = ({
  product,
  onToggle,
}: Readonly<{
  product: Product;
  onToggle: (name: string) => void;
}>) => {
  const price = product.price ? `${product.price} €` : 'N/A';
  return (
    <tr>
      <td>{product.name}</td>
      <td>{product.quantity}</td>
      <td>{price}</td>
      <td>
        <input
          type="checkbox"
          checked={product.checked}
          onChange={() => onToggle(product.name)}
        />
      </td>
    </tr>
  );
};

const ShoppingListTotalRow = ({
  shoppingList,
}: Readonly<{
  shoppingList: ShoppingList;
}>) => {
  return (
    <tr>
      <td>
        <strong>Total</strong>
      </td>
      <td></td>
      <td>{shoppingList.calculateTotal()} €</td>
    </tr>
  );
};

const ShoppingListHeader = () => {
  return (
    <tr>
      <th>Producto</th>
      <th>Cantidad</th>
      <th>Precio</th>
      <th>Marcado</th>
    </tr>
  );
};
